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

        internal KSPTSRegisteredActions registeredActions = new KSPTSRegisteredActions();

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
        }

        void Update()
        {
            CheckIfEOFManagerNeedsReseting();

            List<Action> tmpActionList = registeredActions.inLoop_Update_Actions;

            for (int i = 0; i < tmpActionList.Count; i++)
                _updateThreadPool.EnqueueNewTask(tmpActionList[i]);
        }

        internal void EndUpdate()
        {
            while (_updateThreadPool.busy) ;    //Wait for threadpool to finish
        }

        //This will trigger a reset if the Scene changes or if the number of GOs change.
        void CheckIfEOFManagerNeedsReseting()
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

        internal class KSPTSRegisteredActions
        {
            internal KSPTSRegisteredActions()
            {
                inLoop_Update_Actions = new List<Action>();
                inLoop_LateUpdate_Actions = new List<Action>();
                inLoop_FixedUpdate_Actions = new List<Action>();

                acrossLoop_Update_Actions = new List<Action>();
                acrossLoop_LateUpdate_Actions = new List<Action>();
                acrossLoop_FixedUpdate_Actions = new List<Action>();
            }

            internal List<Action> inLoop_Update_Actions;
            internal List<Action> inLoop_LateUpdate_Actions;
            internal List<Action> inLoop_FixedUpdate_Actions;

            internal List<Action> acrossLoop_Update_Actions;
            internal List<Action> acrossLoop_LateUpdate_Actions;
            internal List<Action> acrossLoop_FixedUpdate_Actions;
        }
    }
}
