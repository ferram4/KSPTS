using System;
using System.Collections.Generic;
using UnityEngine;

namespace KSPThreadingSystem
{
    /// <summary>
    /// Uses KSPAddon to create GameObject that KSPTSThreadController is attached to.  Only ever needs to run once, at startup
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class KSPTSEntryPoint : MonoBehaviour
    {
        void Start()
        {
            if (KSPTSThreadController.instanceExists)
                Debug.LogWarning("KSPTS: Ordered by KSP to create second KSPTSThreadController; disregarding order as unnecessary");
            else
            {
                Debug.Log("Initializing KSPThreadingSystem");
                GameObject o = new GameObject();

                o.AddComponent<KSPTSThreadController>();

                GameObject.DontDestroyOnLoad(o);
            }
        }
    }
}
