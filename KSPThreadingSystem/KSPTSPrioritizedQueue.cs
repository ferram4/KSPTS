using System;
using System.Collections.Generic;
using System.Threading;

namespace KSPThreadingSystem
{
    /// <summary>
    /// A group of queues of tasks with set priorities for each queue; this ensures that
    /// if a certain group fo tasks are causing the main thread to wait, they can be moved
    /// up to be rushed through the worker threads
    /// </summary>
    internal class KSPTSPrioritizedQueue
    {
        private Dictionary<KSPTSThreadingGroup, Queue<KSPTSParametrizedTask>> taskQueues = new Dictionary<KSPTSThreadingGroup,Queue<KSPTSParametrizedTask>>();

        private KSPTSThreadingGroup urgentQueue;
        private bool urgent = false;

        internal KSPTSPrioritizedQueue()
        {
            //There has to be a cleaner way of doing this
            taskQueues.Add(KSPTSThreadingGroup.IN_LOOP_UPDATE, new Queue<KSPTSParametrizedTask>());
            taskQueues.Add(KSPTSThreadingGroup.IN_LOOP_LATE_UPDATE, new Queue<KSPTSParametrizedTask>());
            taskQueues.Add(KSPTSThreadingGroup.IN_LOOP_FIXED_UPDATE, new Queue<KSPTSParametrizedTask>());
            taskQueues.Add(KSPTSThreadingGroup.ACROSS_LOOP_UPDATE, new Queue<KSPTSParametrizedTask>());
            taskQueues.Add(KSPTSThreadingGroup.ACROSS_LOOP_LATE_UPDATE, new Queue<KSPTSParametrizedTask>());
            taskQueues.Add(KSPTSThreadingGroup.ACROSS_LOOP_FIXED_UPDATE, new Queue<KSPTSParametrizedTask>());
        }

        internal void SetUrgent(KSPTSThreadingGroup urgentGroup)
        {
            urgentQueue = urgentGroup;
            urgent = true;
        }

        internal void Enqueue(KSPTSParametrizedTask newTask, KSPTSThreadingGroup group)
        {
            taskQueues[group].Enqueue(newTask);
        }

        internal bool hasTasks()
        {
            return (taskQueues[KSPTSThreadingGroup.IN_LOOP_FIXED_UPDATE].Count > 0 ||
                taskQueues[KSPTSThreadingGroup.ACROSS_LOOP_FIXED_UPDATE].Count > 0 ||
                taskQueues[KSPTSThreadingGroup.IN_LOOP_UPDATE].Count > 0 ||
                taskQueues[KSPTSThreadingGroup.IN_LOOP_LATE_UPDATE].Count > 0 ||
                taskQueues[KSPTSThreadingGroup.ACROSS_LOOP_UPDATE].Count > 0 ||
                taskQueues[KSPTSThreadingGroup.ACROSS_LOOP_LATE_UPDATE].Count > 0);
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

            tmpQueue = taskQueues[KSPTSThreadingGroup.IN_LOOP_FIXED_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                return returnVal;
            }

            tmpQueue = taskQueues[KSPTSThreadingGroup.ACROSS_LOOP_FIXED_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                return returnVal;
            }

            tmpQueue = taskQueues[KSPTSThreadingGroup.IN_LOOP_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                return returnVal;
            }

            tmpQueue = taskQueues[KSPTSThreadingGroup.IN_LOOP_LATE_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                return returnVal;
            }

            tmpQueue = taskQueues[KSPTSThreadingGroup.ACROSS_LOOP_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                return returnVal;
            }

            tmpQueue = taskQueues[KSPTSThreadingGroup.ACROSS_LOOP_LATE_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                return returnVal;
            }
            
            return null;
        }
    }
}
