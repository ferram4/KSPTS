using System;
using System.Collections.Generic;

namespace KSPThreadingSystem
{
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

    internal class KSPTSParametrizedFunc
    {
        internal Func<object, object> action;
        internal object parameter;

        internal KSPTSParametrizedFunc(Func<object, object> func, object parameter)
        {
            this.action = func;
            this.parameter = parameter;
        }
    }
}
