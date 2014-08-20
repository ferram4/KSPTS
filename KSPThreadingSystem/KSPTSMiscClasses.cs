using System;
using System.Collections.Generic;

namespace KSPThreadingSystem
{
    internal class KSPTSRegisteredTasks
    {
        internal KSPTSRegisteredTasks()
        {
            inLoop_Update_Actions = new List<KSPTSTaskGroup>();
            inLoop_LateUpdate_Actions = new List<KSPTSTaskGroup>();
            inLoop_FixedUpdate_Actions = new List<KSPTSTaskGroup>();

            acrossLoop_Update_Actions = new List<KSPTSTaskGroup>();
            acrossLoop_LateUpdate_Actions = new List<KSPTSTaskGroup>();
            acrossLoop_FixedUpdate_Actions = new List<KSPTSTaskGroup>();
        }

        internal List<KSPTSTaskGroup> inLoop_Update_Actions;
        internal List<KSPTSTaskGroup> inLoop_LateUpdate_Actions;
        internal List<KSPTSTaskGroup> inLoop_FixedUpdate_Actions;

        internal List<KSPTSTaskGroup> acrossLoop_Update_Actions;
        internal List<KSPTSTaskGroup> acrossLoop_LateUpdate_Actions;
        internal List<KSPTSTaskGroup> acrossLoop_FixedUpdate_Actions;
    }

    internal class KSPTSTaskGroup
    {
        internal Func<object> preFunction;
        internal Func<object, object> threadedTask;
        internal Action<object> postFunction;

        internal KSPTSTaskGroup(Func<object> preFunction, Func<object, object> threadedTask, Action<object> postFunction)
        {
            this.preFunction = preFunction;
            this.threadedTask = threadedTask;
            this.postFunction = postFunction;
        }
    }

    internal class KSPTSParametrizedPostFunction
    {
        internal object parameter;
        internal Action<object> postFunction;

        internal KSPTSParametrizedPostFunction(Action<object> postFunction, object parameter)
        {
            this.parameter = parameter;
            this.postFunction = postFunction;
        }
    }

    internal class KSPTSParametrizedTask
    {
        internal Func<object, object> action;
        internal object parameter;
        internal Action<object> postFunction;

        internal KSPTSParametrizedTask(Func<object, object> func, object parameter, Action<object> postFunction)
        {
            this.action = func;
            this.parameter = parameter;
            this.postFunction = postFunction;
        }
    }

    /// <summary>
    /// Enum used to specify threading groups and priority; arranged from highest priority to lowest
    /// </summary>
    public enum KSPTSThreadingGroups
    {
        IN_LOOP_UPDATE,
        IN_LOOP_LATE_UPDATE,
        IN_LOOP_FIXED_UPDATE,
        ACROSS_LOOP_UPDATE,
        ACROSS_LOOP_LATE_UPDATE,
        ACROSS_LOOP_FIXED_UPDATE
    }
}
