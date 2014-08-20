using System;
using System.Collections.Generic;
using UnityEngine;

namespace KSPThreadingSystem
{
    /// <summary>
    /// This class exists only to exist at the end of the list of event functions called
    /// by Unity every frame, ensuring that worker threads have completed their tasks
    /// within the appropriate loop.  Should be as lightweight as possible to avoid
    /// producing too much garbage when it is destroyed.
    /// </summary>
    internal class KSPTSEndOfFrameManager : MonoBehaviour
    {
        //int creationId;

        internal KSPTSEndOfFrameManager()
        {
            //creationId = DateTime.Now.GetHashCode();
            Debug.Log("KSPTSEndOfFrameManager has been created");
        }

        void Update()
        {
            //Debug.Log("Test Timing, KSPTSEndOfFrameManager, " + creationId);
            KSPTSThreadController.instance.EndUpdate();
        }
    }
}
