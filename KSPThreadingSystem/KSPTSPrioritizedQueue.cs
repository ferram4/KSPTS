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

        internal bool hasTasks = false;

        private readonly object locker = new object();

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
            lock (locker)
            {
                urgentQueue = urgentGroup;
                urgent = true;
            }
        }

        internal void Enqueue(KSPTSParametrizedTask newTask, KSPTSThreadingGroups group)
        {
            lock (locker)
            {
                taskQueues[group].Enqueue(newTask);
                hasTasks = true;
            }
        }

        internal KSPTSParametrizedTask Dequeue()
        {
            Queue<KSPTSParametrizedTask> tmpQueue = null;
            KSPTSParametrizedTask returnVal = null;
            lock(locker)
                if (urgent)  //The main unity thread is waiting on something, clear everything out
                {
                    tmpQueue = taskQueues[urgentQueue];
                    if (tmpQueue.Count > 0)
                        returnVal = Dequeue();
                    else
                        urgent = false;

                    return returnVal;
                }
                else
                {
                    //for (int i = 0; i < 6; i++)
                    //{
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
                }
            hasTasks = false;

            return null;
        }
    }
}
