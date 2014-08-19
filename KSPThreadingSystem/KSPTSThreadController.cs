using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KSPThreadingSystem
{
    internal class KSPTSThreadController : MonoBehaviour
    {
        internal static KSPTSThreadController instance = null;
        internal static bool instanceExists = false;

        private GameScenes lastScene = GameScenes.LOADING;
        private int lastGOCount = 0;
        private int frameCount = 0;

        private GameObject endOfFrameManagerGO = null;
        private KSPTSEndOfFrameManager endOfFrameManager = null;

        private KSPTSWorkerThreadPool _updateThreadPool;
        private Queue<KSPTSParametrizedPostFunction> _updatePostFunctions;

        private readonly object locker = new object();

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

            _updateThreadPool = new KSPTSWorkerThreadPool((int)(Environment.ProcessorCount * 0.5));
            _updatePostFunctions = new Queue<KSPTSParametrizedPostFunction>();
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

                _updateThreadPool.EnqueueNewTask(tmpTaskGroup.threadedTask, tmpObject, tmpTaskGroup.postFunction);
            }
        }

        internal void EndUpdate()
        {
            while (_updateThreadPool.busy) ;    //Wait for threadpool to finish
        }

        internal void EnqueuePostFunction(Action<object> postFunction, object parameter)
        {
            KSPTSParametrizedPostFunction tmpPostFunc = new KSPTSParametrizedPostFunction(postFunction, parameter);
            lock(locker)
            {
                _updatePostFunctions.Enqueue(tmpPostFunc);
            }
        }

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

        #region EOFManagerResets

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
        }

        #endregion
    }
}
