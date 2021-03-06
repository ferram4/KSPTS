﻿using System;
using System.Collections.Generic;

namespace KSPThreadingSystem
{
    internal class KSPTSRegisteredTasks
    {
        internal Dictionary<KSPTSThreadingGroup, List<KSPTSTaskGroup>> _groupTasks = new Dictionary<KSPTSThreadingGroup, List<KSPTSTaskGroup>>();

        internal KSPTSRegisteredTasks()
        {
            _groupTasks.Add(KSPTSThreadingGroup.IN_LOOP_UPDATE, new List<KSPTSTaskGroup>());
            _groupTasks.Add(KSPTSThreadingGroup.IN_LOOP_LATE_UPDATE, new List<KSPTSTaskGroup>());
            _groupTasks.Add(KSPTSThreadingGroup.IN_LOOP_FIXED_UPDATE, new List<KSPTSTaskGroup>());
            _groupTasks.Add(KSPTSThreadingGroup.ACROSS_LOOP_UPDATE, new List<KSPTSTaskGroup>());
            _groupTasks.Add(KSPTSThreadingGroup.ACROSS_LOOP_LATE_UPDATE, new List<KSPTSTaskGroup>());
            _groupTasks.Add(KSPTSThreadingGroup.ACROSS_LOOP_FIXED_UPDATE, new List<KSPTSTaskGroup>());
        }

        /// <summary>
        /// Called on scene changes to ensure that all references to objects from the previous scene are removed; this prevents memory leaks
        /// </summary>
        internal void SceneChangeClearAllRegisteredTasks()
        {
            foreach (KeyValuePair<KSPTSThreadingGroup, List<KSPTSTaskGroup>> pair in _groupTasks)
                pair.Value.Clear();
        }
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
        internal KSPTSThreadingGroup threadingGroup;

        internal KSPTSParametrizedTask(Func<object, object> func, object parameter, Action<object> postFunction, KSPTSThreadingGroup threadingGroup)
        {
            this.action = func;
            this.parameter = parameter;
            this.postFunction = postFunction;
            this.threadingGroup = threadingGroup;
        }
    }

    /// <summary>
    /// Enum used to specify threading groups
    /// </summary>
    public enum KSPTSThreadingGroup
    {
        IN_LOOP_UPDATE,
        IN_LOOP_LATE_UPDATE,
        IN_LOOP_FIXED_UPDATE,
        ACROSS_LOOP_UPDATE,
        ACROSS_LOOP_LATE_UPDATE,
        ACROSS_LOOP_FIXED_UPDATE
    }
}
