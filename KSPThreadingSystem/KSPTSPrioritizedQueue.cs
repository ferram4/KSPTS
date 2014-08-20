using System;
using System.Collections.Generic;
using System.Threading;

namespace KSPThreadingSystem
{
    internal class KSPTSPrioritizedQueue
    {
        private Dictionary<KSPTSThreadingGroups, Queue<KSPTSParametrizedTask>> taskQueues = new Dictionary<KSPTSThreadingGroups,Queue<KSPTSParametrizedTask>>();

        private KSPTSThreadingGroups urgentQueue;
        private bool urgent = false;

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

        internal void SetUrgent(KSPTSThreadingGroups urgentGroup)
        {
            urgentQueue = urgentGroup;
            urgent = true;
        }

        internal void Enqueue(KSPTSParametrizedTask newTask, KSPTSThreadingGroups group)
        {
            taskQueues[group].Enqueue(newTask);
        }

        internal bool hasTasks()
        {
            return (taskQueues[KSPTSThreadingGroups.IN_LOOP_FIXED_UPDATE].Count > 0 ||
                taskQueues[KSPTSThreadingGroups.ACROSS_LOOP_FIXED_UPDATE].Count > 0 ||
                taskQueues[KSPTSThreadingGroups.IN_LOOP_UPDATE].Count > 0 ||
                taskQueues[KSPTSThreadingGroups.IN_LOOP_LATE_UPDATE].Count > 0 ||
                taskQueues[KSPTSThreadingGroups.ACROSS_LOOP_UPDATE].Count > 0 ||
                taskQueues[KSPTSThreadingGroups.ACROSS_LOOP_LATE_UPDATE].Count > 0);
        }

        internal KSPTSParametrizedTask Dequeue()
        {
            Queue<KSPTSParametrizedTask> tmpQueue = null;
            KSPTSParametrizedTask returnVal = null;
            if (urgent)  //The main unity thread is waiting on something, clear everything out
            {
                tmpQueue = taskQueues[urgentQueue];
                if (tmpQueue.Count > 0)
                    return tmpQueue.Dequeue();
                else
                    urgent = false;
            }

            tmpQueue = taskQueues[KSPTSThreadingGroups.IN_LOOP_FIXED_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                return returnVal;
            }

            tmpQueue = taskQueues[KSPTSThreadingGroups.ACROSS_LOOP_FIXED_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                return returnVal;
            }

            tmpQueue = taskQueues[KSPTSThreadingGroups.IN_LOOP_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                return returnVal;
            }

            tmpQueue = taskQueues[KSPTSThreadingGroups.IN_LOOP_LATE_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                return returnVal;
            }

            tmpQueue = taskQueues[KSPTSThreadingGroups.ACROSS_LOOP_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                return returnVal;
            }

            tmpQueue = taskQueues[KSPTSThreadingGroups.ACROSS_LOOP_LATE_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                return returnVal;
            }
            
            return null;
        }
    }
}
