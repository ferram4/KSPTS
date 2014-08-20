using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KSPThreadingSystem
{
    /// <summary>
    /// This class handles all the worker threads used by KSPTS.  Actions are queued up, and then workers dequeue and execute those actions until none remain
    /// </summary>
    internal class KSPTSWorkerThreadPool
    {
        private Thread[] _threads;
        private KSPTSPrioritizedQueue _tasks = new KSPTSPrioritizedQueue();
        private readonly object locker = new object();

        internal KSPTSWorkerThreadPool() : this(Environment.ProcessorCount) { }

        internal KSPTSWorkerThreadPool(int numThreads)
        {
            _threads = new Thread[numThreads];

            for (int i = 0; i < numThreads; i++)
                (_threads[i] = new Thread(DoTasks)).Start();
        }

        internal void EnqueueNewTask(Func<object, object> newTask, object newParameter, Action<object> newPostFunction, KSPTSThreadingGroup group)
        {
            KSPTSParametrizedTask _paraAction = new KSPTSParametrizedTask(newTask, newParameter, newPostFunction, group);
            lock(locker)
            {
                _tasks.Enqueue(_paraAction, group);
                Monitor.Pulse(locker);
            }
        }

        internal void SetUrgent(KSPTSThreadingGroup urgentGroup)
        {
            lock (locker)
            {
                _tasks.SetUrgent(urgentGroup);
            }
        }

        private void DoTasks()
        {
            while(true)
            {
                KSPTSParametrizedTask currentTask = null;

                lock (locker)
                {
                    while (!_tasks.hasTasks())
                        Monitor.Wait(locker);

                    currentTask = _tasks.Dequeue();
                }

                if (currentTask.action == null)
                {
                    KSPTSThreadController.instance.EnqueuePostFunction(null, null, currentTask.threadingGroup);
                    continue;
                }

                object postFuncParam = currentTask.action(currentTask.parameter);

                KSPTSThreadController.instance.EnqueuePostFunction(currentTask.postFunction, postFuncParam, currentTask.threadingGroup);
            }
        }
    }
}
