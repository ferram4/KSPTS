using System;
using System.Collections.Generic;
using UnityEngine;

namespace KSPThreadingSystem
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class KSPTSEntryPoint : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("Initializing KSPThreadingSystem");
            GameObject o = new GameObject();

            o.AddComponent<KSPTSThreadController>();

            GameObject.DontDestroyOnLoad(o);
        }
    }
}
