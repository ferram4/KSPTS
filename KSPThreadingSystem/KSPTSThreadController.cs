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
        /*private int _updateNumPostFuncsRemaining = 0;
        private int _lateUpdateNumPostFuncsRemaining = 0;
        private int _fixedUpdateNumPostFuncsRemaining = 0;
        private int _acrossUpdateNumPostFuncsRemaining = 0;
        private int _acrossLateUpdateNumPostFuncsRemaining = 0;
        private int _acrossFixedUpdateNumPostFuncsRemaining = 0;*/

        
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

        void Update()
        {
            CheckIfEOFManagerNeedsResetting();

            List<KSPTSTaskGroup> tmpTaskGroupList = registeredTasks.inLoop_Update_Actions;

            for (int i = 0; i < tmpTaskGroupList.Count; i++)
            {
                KSPTSTaskGroup tmpTaskGroup = tmpTaskGroupList[i];
                object tmpObject = null;
                if (tmpTaskGroup.preFunction != null)
                    tmpObject = tmpTaskGroup.preFunction();

                _threadPool.EnqueueNewTask(tmpTaskGroup.threadedTask, tmpObject, tmpTaskGroup.postFunction, KSPTSThreadingGroup.IN_LOOP_UPDATE);
            }
            _postFunctionsRemaining[KSPTSThreadingGroup.IN_LOOP_UPDATE] = tmpTaskGroupList.Count;
        }

        internal void EndUpdate()
        {
            WaitForTheadingGroupToFinish(KSPTSThreadingGroup.IN_LOOP_UPDATE);
        }

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

        #region EOFManagerResets

        //This will trigger a reset if the Scene changes or if the number of GOs change.
        void CheckIfEOFManagerNeedsResetting()
        {
            if (HighLogic.LoadedScene != lastScene)
            {
                lastScene = HighLogic.LoadedScene;
                Debug.Log("KSPTSThreadController reporting; scene: " + HighLogic.LoadedScene.ToString());
                ResetEndOfFrameManager();
            }

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
