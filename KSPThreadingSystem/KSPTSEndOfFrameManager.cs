using System;
using System.Collections.Generic;
using UnityEngine;

namespace KSPThreadingSystem
{
    internal class KSPTSEndOfFrameManager : MonoBehaviour
    {
        internal KSPTSEndOfFrameManager()
        {
            Debug.Log("KSPTSEndOfFrameManager has been created");
        }

        void Update()
        {
            KSPTSThreadController.instance.EndUpdate();
            //Debug.Log("Test Timing, KSPTSEndOfFrameManager");
        }
    }
}
