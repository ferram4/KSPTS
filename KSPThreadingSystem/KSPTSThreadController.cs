using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KSPThreadingSystem
{
    public class KSPTSThreadController : MonoBehaviour
    {
        public static KSPTSThreadController instance = null;
        public static bool instanceExists = false;

        private GameScenes lastScene = GameScenes.LOADING;
        private KSPTSEndOfFrameManager endOfFrameManager = null;
        private List<Thread> threads = null;

        public KSPTSThreadController()
        {
            Debug.Log("KSPTSThreadController Created");
            instance = this;
            threads = new List<Thread>();
        }

        void Start()
        {
            Debug.Log("KSPTSThreadController registering with KSP events");
            GameEvents.onVesselCreate.Add(ResetEndOfFrameManager);
            GameEvents.onVesselGoOffRails.Add(ResetEndOfFrameManager);
            GameEvents.onVesselGoOnRails.Add(ResetEndOfFrameManager);
            GameEvents.onVesselWasModified.Add(ResetEndOfFrameManager);
            GameEvents.onEditorShipModified.Add(ResetEndOfFrameManager);
        }

        void Update()
        {
            if(HighLogic.LoadedScene != lastScene)
            {
                lastScene = HighLogic.LoadedScene;
                Debug.Log("KSPTSThreadController reporting; scene: " + HighLogic.LoadedScene.ToString());
                ResetEndOfFrameManager();
            }
        }

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
            endOfFrameManager = null;
            // TODO: Implement stopping of all threads to allow re-construction of KSPTSEndOfFrameManager
            endOfFrameManager = new KSPTSEndOfFrameManager();
        }
    }
}
