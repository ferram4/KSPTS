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

        //Updating values
        private GameScenes lastScene = GameScenes.LOADING;
        private int lastGOCount = 0;
        private int frameCount = 0;

        private GameObject endOfFrameManagerGO = null;
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

            if(frameCount > 300)
            {
                int tmpGOCount = Resources.FindObjectsOfTypeAll<GameObject>().Length;
                if(tmpGOCount != lastGOCount)
                {
                    lastGOCount = tmpGOCount;
                    ResetEndOfFrameManager();
                }
                frameCount = 0;
            }
            frameCount++;
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
            // TODO: Implement stopping of all threads to allow re-construction of KSPTSEndOfFrameManager

            GameObject.Destroy(endOfFrameManagerGO);
            endOfFrameManager = null;

            endOfFrameManagerGO = new GameObject();
            endOfFrameManagerGO.AddComponent<KSPTSEndOfFrameManager>();

            endOfFrameManager = endOfFrameManagerGO.GetComponent<KSPTSEndOfFrameManager>();
        }
    }
}
