using System;
using System.Collections.Generic;
using UnityEngine;
using KSPThreadingSystem;

namespace KSPTSDemo
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class KSPTSDemoController : MonoBehaviour
    {
        KSPTSDemoObject[] array;

        const int c = 4;
        readonly object locker = new object();

        void Start()
        {
            Debug.Log("Starting KSPTS Demo");
            array = new KSPTSDemoObject[c];
            for (int i = 0; i < c; i++)
                array[i] = new KSPTSDemoObject(i);
        }

        /*void Update()
        {
            string s = "";
            for (int i = 0; i < c; i++)
                s += "Index " + i + ": " + array[i];

            Debug.Log(s);
        }*/
    }

    public class KSPTSDemoObject
    {
        int val;
        //System.Random rand;

        public KSPTSDemoObject(int val)
        {
            this.val = val;
            //rand = new System.Random(val * System.DateTime.Now.Millisecond);

            KSPTSAPI.RegisterNewThreadTask(KSPTSThreadingGroup.IN_LOOP_UPDATE, PreFunction, ThreadedTask, PostFunction);
        }

        public object PreFunction()
        {
            Debug.Log("Running PreFunction");
            return val;
        }

        public object ThreadedTask(object parameter)
        {
            int value = (int)parameter;

            int output = 25 * value;

            return output;
        }

        public void PostFunction(object parameter)
        {
            Debug.Log("Running PostFunction; output of threadedTask is: " + parameter);
        }
    }
}
