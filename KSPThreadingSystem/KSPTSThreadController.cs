using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KSPThreadingSystem
{
    /// <summary>
    /// Controller class for the entire KSPTS.  Attached to a GameObject that is set to 
    /// persist throughout the lifetime of the game; this ensures that the 
    /// ThreadController is one of the first classes to execute when its Unity event
    /// functions (Update, FixedUpdate, LateUpdate) are called.
    /// </summary>
    internal class KSPTSThreadController : MonoBehaviour
    {
        internal static KSPTSThreadController instance = null;
        internal static bool instanceExists = false;

        GameScenes lastScene = GameScenes.LOADING;
        int lastGOCount = 0;
        int frameCount = 0;

        GameObject endOfFrameManagerGO = null;
        KSPTSEndOfFrameManager endOfFrameManager = null;

        KSPTSWorkerThreadPool _threadPool;

        Dictionary<KSPTSThreadingGroup, Queue<KSPTSParametrizedPostFunction>> _postFunctions = new Dictionary<KSPTSThreadingGroup, Queue<KSPTSParametrizedPostFunction>>();
        Dictionary<KSPTSThreadingGroup, int> _postFunctionsRemaining = new Dictionary<KSPTSThreadingGroup, int>();
        
        readonly object locker = new object();

        internal KSPTSRegisteredTasks registeredTasks = new KSPTSRegisteredTasks();

        internal KSPTSThreadController()
        {
            Debug.Log("KSPTSThreadController Created");
            instance = this;
        }

        void Start()
        {
            Debug.Log("KSPTSThreadController registering with KSP events");
            GameEvents.onVesselCreate.Add(ResetEndOfFrameManager);
            GameEvents.onVesselGoOffRails.Add(ResetEndOfFrameManager);
            GameEvents.onVesselGoOnRails.Add(ResetEndOfFrameManager);
            GameEvents.onVesselWasModified.Add(ResetEndOfFrameManager);
            GameEvents.onEditorShipModified.Add(ResetEndOfFrameManager);

            _threadPool = new KSPTSWorkerThreadPool((Environment.ProcessorCount));

            //There has to be a cleaner way of doing this
            _postFunctions.Add(KSPTSThreadingGroup.IN_LOOP_UPDATE, new Queue<KSPTSParametrizedPostFunction>());
            _postFunctions.Add(KSPTSThreadingGroup.IN_LOOP_LATE_UPDATE, new Queue<KSPTSParametrizedPostFunction>());
            _postFunctions.Add(KSPTSThreadingGroup.IN_LOOP_FIXED_UPDATE, new Queue<KSPTSParametrizedPostFunction>());
            _postFunctions.Add(KSPTSThreadingGroup.ACROSS_LOOP_UPDATE, new Queue<KSPTSParametrizedPostFunction>());
            _postFunctions.Add(KSPTSThreadingGroup.ACROSS_LOOP_LATE_UPDATE, new Queue<KSPTSParametrizedPostFunction>());
            _postFunctions.Add(KSPTSThreadingGroup.ACROSS_LOOP_FIXED_UPDATE, new Queue<KSPTSParametrizedPostFunction>());

        }

        #region Unity Functions

        void Update()
        {
            //First, wait for all the ACROSS LOOP tasks from last time to finish
            WaitForTheadingGroupToFinish(KSPTSThreadingGroup.ACROSS_LOOP_UPDATE);

            CheckIfEOFManagerNeedsResetting();

            //Then, queue all the tasks needed to be finished by the end of this loop
            QueueThreadingGroupTasks(KSPTSThreadingGroup.IN_LOOP_UPDATE);

            //Finally, queue all the tasks that are needed by the end of next loop
            QueueThreadingGroupTasks(KSPTSThreadingGroup.ACROSS_LOOP_UPDATE);
        }

        internal void EndUpdate()
        {
            WaitForTheadingGroupToFinish(KSPTSThreadingGroup.IN_LOOP_UPDATE);
        }

        void LateUpdate()
        {
            //First, wait for all the ACROSS LOOP tasks from last time to finish
            WaitForTheadingGroupToFinish(KSPTSThreadingGroup.ACROSS_LOOP_LATE_UPDATE);

            //Then, queue all the tasks needed to be finished by the end of this loop
            QueueThreadingGroupTasks(KSPTSThreadingGroup.IN_LOOP_LATE_UPDATE);

            //Finally, queue all the tasks that are needed by the end of next loop
            QueueThreadingGroupTasks(KSPTSThreadingGroup.ACROSS_LOOP_LATE_UPDATE);
        }

        internal void EndLateUpdate()
        {
            WaitForTheadingGroupToFinish(KSPTSThreadingGroup.IN_LOOP_LATE_UPDATE);
        }

        void FixedUpdate()
        {
            //First, wait for all the ACROSS LOOP tasks from last time to finish
            WaitForTheadingGroupToFinish(KSPTSThreadingGroup.ACROSS_LOOP_FIXED_UPDATE);

            //Then, queue all the tasks needed to be finished by the end of this loop
            QueueThreadingGroupTasks(KSPTSThreadingGroup.IN_LOOP_FIXED_UPDATE);

            //Finally, queue all the tasks that are needed by the end of next loop
            QueueThreadingGroupTasks(KSPTSThreadingGroup.ACROSS_LOOP_FIXED_UPDATE);
        }

        internal void EndFixedUpdate()
        {
            WaitForTheadingGroupToFinish(KSPTSThreadingGroup.IN_LOOP_FIXED_UPDATE);
        }

        #endregion

        /// <summary>
        /// Queues up post-function with relevant data for execution in WaitForTheadingGroupToFinish()
        /// </summary>
        /// <param name="postFunction">function to be executed using data from completed threadedTask</param>
        /// <param name="parameter">data from completed threadedTask</param>
        /// <param name="group">this function's thread group</param>
        internal void EnqueuePostFunction(Action<object> postFunction, object parameter, KSPTSThreadingGroup group)
        {
            KSPTSParametrizedPostFunction tmpPostFunc = new KSPTSParametrizedPostFunction(postFunction, parameter);
            Queue<KSPTSParametrizedPostFunction> tmpQueue = _postFunctions[group];

            lock(locker)
            {
                tmpQueue.Enqueue(tmpPostFunc);
                Monitor.Pulse(locker);
            }
        }

        #region Thread Tasking Functions

        /// <summary>
        /// Queues up all registered tasks and sends them to the thread pool to be processed
        /// </summary>
        /// <param name="group">Threading group to queue tasks for</param>
        void QueueThreadingGroupTasks(KSPTSThreadingGroup group)
        {
            List<KSPTSTaskGroup> tmpTaskGroupList = registeredTasks._groupTasks[group];

            for (int i = 0; i < tmpTaskGroupList.Count; i++)
            {
                KSPTSTaskGroup tmpTaskGroup = tmpTaskGroupList[i];
                object tmpObject = null;
                if (tmpTaskGroup.preFunction != null)
                    tmpObject = tmpTaskGroup.preFunction();

                _threadPool.EnqueueNewTask(tmpTaskGroup.threadedTask, tmpObject, tmpTaskGroup.postFunction, group);
            }
            _postFunctionsRemaining[group] = tmpTaskGroupList.Count;

        }

        /// <summary>
        /// Waits for tasks to finish, executes post-functions and then returns control to Unity and the rest of KSP
        /// </summary>
        /// <param name="group">Threading group being waited on</param>
        void WaitForTheadingGroupToFinish(KSPTSThreadingGroup group)
        {
            _threadPool.SetUrgent(group);

            int numPostFuncsRemaining = _postFunctionsRemaining[group];
            Queue<KSPTSParametrizedPostFunction> postFunctionQueue = _postFunctions[group];

            while (numPostFuncsRemaining > 0)
            {
                KSPTSParametrizedPostFunction tmp;
                lock (locker)
                {
                    while (postFunctionQueue.Count == 0)
                        Monitor.Wait(locker);

                    tmp = postFunctionQueue.Dequeue();
                }
                numPostFuncsRemaining--;
                if (tmp.postFunction != null)
                    tmp.postFunction(tmp.parameter);

            }
        }

        #endregion

        #region EOFManagerResets

        //This will trigger a reset if the Scene changes or if the number of GOs change.
        void CheckIfEOFManagerNeedsResetting()
        {
            if (HighLogic.LoadedScene != lastScene)
            {
                lastScene = HighLogic.LoadedScene;
                Debug.Log("KSPTSThreadController reporting; scene: " + HighLogic.LoadedScene.ToString());
                ResetEndOfFrameManager();
                registeredTasks.SceneChangeClearAllRegisteredTasks();
            }

            //Since finding all GameObjects is somewhat expensive, only do this every 90 frames
            if (frameCount > 90)
            {
                int tmpGOCount = Resources.FindObjectsOfTypeAll<GameObject>().Length;
                if (tmpGOCount != lastGOCount)
                {
                    lastGOCount = tmpGOCount;
                    ResetEndOfFrameManager();
                }
                frameCount = 0;
            }
            frameCount++;
        }

        //These are used to destroy and recreate the EOFManager that handles threads that must end in the same frame they started in
        void ResetEndOfFrameManager(ShipConstruct v)
        {
            ResetEndOfFrameManager();
        }

        void ResetEndOfFrameManager(Vessel v)
        {
            ResetEndOfFrameManager();
        }

        void ResetEndOfFrameManager()
        {
            // TODO: Implement stopping of all threads to allow re-construction of KSPTSEndOfFrameManager

            GameObject.Destroy(endOfFrameManagerGO);
            endOfFrameManager = null;

            endOfFrameManagerGO = new GameObject();
            endOfFrameManagerGO.AddComponent<KSPTSEndOfFrameManager>();

            endOfFrameManager = endOfFrameManagerGO.GetComponent<KSPTSEndOfFrameManager>();

            GameObject.DontDestroyOnLoad(endOfFrameManagerGO);
        }

        #endregion
    }
}
