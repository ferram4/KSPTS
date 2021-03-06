﻿using System;
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

        private bool _hasTasks = false;

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

        //This is used for setting ACROSS_LOOP updates to finish by the end of the frame, but is set up to accept any switching
        internal void SwapQueues(KSPTSThreadingGroup group1, KSPTSThreadingGroup group2)
        {
            Queue<KSPTSParametrizedTask> tmpQueue = taskQueues[group1];
            taskQueues[group1] = taskQueues[group2];
            taskQueues[group2] = tmpQueue;
        }

        internal void SetUrgent(KSPTSThreadingGroup urgentGroup)
        {
            urgentQueue = urgentGroup;
            urgent = true;
        }

        internal void Enqueue(KSPTSParametrizedTask newTask, KSPTSThreadingGroup group)
        {
            taskQueues[group].Enqueue(newTask);
            _hasTasks = true;
        }

        internal bool hasTasks()
        {
            return _hasTasks;/*(taskQueues[KSPTSThreadingGroup.IN_LOOP_FIXED_UPDATE].Count > 0 ||
                taskQueues[KSPTSThreadingGroup.ACROSS_LOOP_FIXED_UPDATE].Count > 0 ||
                taskQueues[KSPTSThreadingGroup.IN_LOOP_UPDATE].Count > 0 ||
                taskQueues[KSPTSThreadingGroup.IN_LOOP_LATE_UPDATE].Count > 0 ||
                taskQueues[KSPTSThreadingGroup.ACROSS_LOOP_UPDATE].Count > 0 ||
                taskQueues[KSPTSThreadingGroup.ACROSS_LOOP_LATE_UPDATE].Count > 0);*/
        }

        private bool privateTaskCheck()
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
                {
                    returnVal = tmpQueue.Dequeue();
                    _hasTasks = privateTaskCheck();
                    return returnVal;
                }
                else
                    urgent = false;
            }

            tmpQueue = taskQueues[KSPTSThreadingGroup.IN_LOOP_FIXED_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                _hasTasks = privateTaskCheck();
                return returnVal;
            }

            tmpQueue = taskQueues[KSPTSThreadingGroup.ACROSS_LOOP_FIXED_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                _hasTasks = privateTaskCheck();
                return returnVal;
            }

            tmpQueue = taskQueues[KSPTSThreadingGroup.IN_LOOP_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                _hasTasks = privateTaskCheck();
                return returnVal;
            }

            tmpQueue = taskQueues[KSPTSThreadingGroup.IN_LOOP_LATE_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                _hasTasks = privateTaskCheck();
                return returnVal;
            }

            tmpQueue = taskQueues[KSPTSThreadingGroup.ACROSS_LOOP_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                _hasTasks = privateTaskCheck();
                return returnVal;
            }

            tmpQueue = taskQueues[KSPTSThreadingGroup.ACROSS_LOOP_LATE_UPDATE];
            if (tmpQueue.Count > 0)
            {
                returnVal = tmpQueue.Dequeue();
                _hasTasks = privateTaskCheck();
                return returnVal;
            }

            return null;
        }
    }
}
