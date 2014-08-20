using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSPThreadingSystem
{
    /// <summary>
    /// Used to interface with KSPTS to prevent interference between plugins and the system
    /// </summary>
    public static class KSPTSAPI
    {
        #region RegistrationFunctions

        /// <summary>
        /// Register a task without pre- or post- functions
        /// </summary>
        /// <param name="threadingGroup">Determines which loop to synchronize with and when</param>
        /// <param name="threadedTask">Task that takes an object as a parameter and returns an object</param>
        public static void RegisterNewThreadTask(KSPTSThreadingGroup threadingGroup, Func<object, object> threadedTask)
        {
            RegisterNewThreadTask(threadingGroup, null, threadedTask, null);
        }

        /// <summary>
        /// Register a task with a pre-function to run in the main Unity thread before starting the threadedTask
        /// </summary>
        /// <param name="threadingGroup">Determines which loop to synchronize with and when</param>
        /// <param name="preFunction">Method that returns an object passed to the threadedTask as a parameter; run in the main Unity thread before threadedTask is started</param>
        /// <param name="threadedTask">Task that takes an object as a parameter and returns an object</param>
        public static void RegisterNewThreadTask(KSPTSThreadingGroup threadingGroup, Func<object> preFunction, Func<object, object> threadedTask)
        {
            RegisterNewThreadTask(threadingGroup, preFunction, threadedTask, null);
        }

        /// <summary>
        /// Register a task with a post-function to run in the main Unity thread after finishing the threadedTask
        /// </summary>
        /// <param name="threadingGroup">Determines which loop to synchronize with and when</param>
        /// <param name="threadedTask">Task that takes an object as a parameter and returns an object</param>
        /// <param name="postFunction">Void Method that takes an object as a parameter and is run in the main Unity thread after threadedTask completes</param>
        public static void RegisterNewThreadTask(KSPTSThreadingGroup threadingGroup, Func<object, object> threadedTask, Action<object> postFunction)
        {
            RegisterNewThreadTask(threadingGroup, null, threadedTask, postFunction);
        }

        /// <summary>
        /// Register a task with a pre-function to run in the main Unity thread before starting the threadedTask and a post-function to run in the main Unity thread after finishing the threadedTask
        /// </summary>
        /// <param name="threadingGroup">Determines which loop to synchronize with and when</param>
        /// <param name="preFunction">Method that returns an object passed to the threadedTask as a parameter; run in the main Unity thread before threadedTask is started</param>
        /// <param name="threadedTask">Task that takes an object as a parameter and returns an object</param>
        /// <param name="postFunction">Void Method that takes an object as a parameter and is run in the main Unity thread after threadedTask completes</param>
        public static void RegisterNewThreadTask(KSPTSThreadingGroup threadingGroup, Func<object> preFunction, Func<object, object> threadedTask, Action<object> postFunction)
        {
            KSPTSTaskGroup newTaskGroup = new KSPTSTaskGroup(preFunction, threadedTask, postFunction);
            RegisterNewThreadTask(threadingGroup, newTaskGroup);
        }

        /// <summary>
        /// Internal task registration method
        /// </summary>
        /// <param name="threadingGroup">Determines which loop to synchronize with and when</param>
        /// <param name="newTaskGroup">Class holding all pre-function, threadedTask, and post-function data</param>
        internal static void RegisterNewThreadTask(KSPTSThreadingGroup threadingGroup, KSPTSTaskGroup newTaskGroup)
        {
            KSPTSRegisteredTasks registeredTasks = KSPTSThreadController.instance.registeredTasks;

            registeredTasks._groupTasks[threadingGroup].Add(newTaskGroup);
        }

        #endregion

        /// <summary>
        /// Attempts to unregister threadedTask from the specified threadingGroup
        /// </summary>
        /// <param name="threadingGroup">Determines which loop to synchronize with and when</param>
        /// <param name="threadedTask">Task that takes an object as a parameter and returns an object</param>
        /// <returns>Success?</returns>
        public static bool TryUnregisterThreadTask(KSPTSThreadingGroup threadingGroup, Func<object, object> threadedTask)
        {
            KSPTSRegisteredTasks registeredTasks = KSPTSThreadController.instance.registeredTasks;

            List<KSPTSTaskGroup> taskGroupList = null;

            if(registeredTasks._groupTasks.TryGetValue(threadingGroup, out taskGroupList))
                return TryRemoveThreadedTask(taskGroupList, threadedTask);

            Debug.LogError("KSPTS Error: Could not find specified Threading Group Task List");
            return false;
        }

        static bool TryRemoveThreadedTask(List<KSPTSTaskGroup> taskGroupList, Func<object, object> threadedTask)
        {
            for(int i = 0; i < taskGroupList.Count; i++)
            {
                KSPTSTaskGroup tmpTaskGroup = taskGroupList[i];
                if(threadedTask.Equals(tmpTaskGroup.threadedTask))
                {
                    taskGroupList.Remove(tmpTaskGroup);
                    return true;
                }
            }

            return false;
        }
    }
}
