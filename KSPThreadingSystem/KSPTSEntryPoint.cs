using System;
using System.Collections.Generic;
using UnityEngine;

namespace KSPThreadingSystem
{
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
