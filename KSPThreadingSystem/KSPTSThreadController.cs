using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KSPThreadingSystem
{
    public class KSPTSThreadController : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("KSPTSThreadController Initialized");
        }

        void Update()
        {
            Debug.Log("KSPTSThreadController reporting; scene: " + HighLogic.LoadedScene.ToString());
        }
    }
}
