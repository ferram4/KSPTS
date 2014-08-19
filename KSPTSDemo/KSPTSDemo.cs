using System;
using System.Collections.Generic;
using UnityEngine;

namespace KSPTSDemo
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class KSPTSDemo : MonoBehaviour
    {
        int[] array;

        const int c = 15;
        readonly object locker = new object();

        void Start()
        {
            Debug.Log("Starting KSPTS Demo");
            array = new int[c];
            for (int i = 0; i < c; i++)
                array[i] = i;
        }

        void Update()
        {
            string s = "";
            for (int i = 0; i < c; i++)
                s += "Index " + i + ": " + array[i];

            Debug.Log(s);
        }

        void CalculateRandom()
        {
        }
    }
}
