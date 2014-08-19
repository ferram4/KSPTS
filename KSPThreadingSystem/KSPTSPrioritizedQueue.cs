using System;
using System.Collections.Generic;

namespace KSPThreadingSystem
{
    internal class KSPTSPrioritizedQueue
    {
        private Dictionary<KSPTSThreadingGroups, Queue<KSPTSParametrizedTask>> taskQueues = new Dictionary<KSPTSThreadingGroups,Queue<KSPTSParametrizedTask>>();

        internal KSPTSPrioritizedQueue()
        {
            //There has to be a cleaner way of doing this
            taskQueues.Add(KSPTSThreadingGroups.IN_LOOP_UPDATE, new Queue<KSPTSParametrizedTask>());
            taskQueues.Add(KSPTSThreadingGroups.IN_LOOP_LATE_UPDATE, new Queue<KSPTSParametrizedTask>());
            taskQueues.Add(KSPTSThreadingGroups.IN_LOOP_FIXED_UPDATE, new Queue<KSPTSParametrizedTask>());
            taskQueues.Add(KSPTSThreadingGroups.ACROSS_LOOP_UPDATE, new Queue<KSPTSParametrizedTask>());
            taskQueues.Add(KSPTSThreadingGroups.ACROSS_LOOP_LATE_UPDATE, new Queue<KSPTSParametrizedTask>());
            taskQueues.Add(KSPTSThreadingGroups.ACROSS_LOOP_FIXED_UPDATE, new Queue<KSPTSParametrizedTask>());
        }

        internal void Enqueue(KSPTSParametrizedTask newTask, KSPTSThreadingGroups group)
        {
            taskQueues[group].Enqueue(newTask);
        }
    }
}
