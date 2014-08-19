using System;
using System.Collections.Generic;
using UnityEngine;

namespace KSPThreadingSystem
{
    /// <summary>
    /// This class exists only to exist at the end of the list of event functions called by Unity every frame, ensuring that worker threads have completed their tasks within the appropriate loop
    /// </summary>
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
