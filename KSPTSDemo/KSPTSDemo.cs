using System;
using System.Collections.Generic;
using UnityEngine;
using KSPThreadingSystem;

namespace KSPTSDemo
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class KSPTSDemoController : MonoBehaviour
    {
        private Rect windowRect = new Rect(100, 100, 300, 300);
        List<KSPTSDemoObject> list;

        const int c = 4;
        readonly object locker = new object();

        void Start()
        {
            Debug.Log("Starting KSPTS Demo");
            list = new List<KSPTSDemoObject>();
            for (int i = 0; i < c; i++)
                list.Add(new KSPTSDemoObject(i));

            this.enabled = true;
        }

        public void OnGUI()
        {
            windowRect = GUI.Window(this.GetHashCode(), windowRect, ExceptionThrowerGUI, "KSPTS Demo");
        }

        private void ExceptionThrowerGUI(int id)
        {
            GUILayout.Label("Use this to add and remove tasks from OnUpdate");
            if (GUILayout.Button("Add New Task"))
            {
                list.Add(new KSPTSDemoObject(list.Count));
            }
            if (GUILayout.Button("Remove Last Task"))
            {
                KSPTSDemoObject tmp = list[list.Count - 1];
                list.Remove(tmp);
                tmp.UnRegister();
            }
            string s = "Output:\n\r";
            for (int i = 0; i < list.Count; i++)
            {
                s += (i + " " + list[i].output + "\n\r");
            }
            GUILayout.Label(s);
            GUI.DragWindow();
        }

    }

    public class KSPTSDemoObject
    {
        int val;
        public string output = "";
        System.Random rand;

        public KSPTSDemoObject(int val)
        {
            this.val = val;
            rand = new System.Random(val * System.DateTime.Now.Millisecond);

            KSPTSAPI.RegisterNewThreadTask(KSPTSThreadingGroup.IN_LOOP_UPDATE, PreFunction, ThreadedTask, PostFunction);
        }

        public object PreFunction()
        {
            //Debug.Log("Running PreFunction");
            return val;
        }

        public object ThreadedTask(object parameter)
        {
            int value = (int)parameter;

            int output = rand.Next(25) * value;

            return output;
        }

        public void PostFunction(object parameter)
        {
            //Debug.Log("Running PostFunction");
            output = parameter.ToString();
        }

        public bool UnRegister()
        {
            return KSPTSAPI.TryUnregisterThreadTask(KSPTSThreadingGroup.IN_LOOP_UPDATE, ThreadedTask);
        }
    }
}
