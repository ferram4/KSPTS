using System;
using System.Collections.Generic;
using System.Threading;

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

        private bool busy;

        internal KSPTSWorkerThreadPool() : this(Environment.ProcessorCount) { }

        internal KSPTSWorkerThreadPool(int numThreads)
        {
            _threads = new Thread[numThreads];

            for (int i = 0; i < numThreads; i++)
                (_threads[i] = new Thread(DoTasks)).Start();
        }

        internal void EnqueueNewTask(Func<object, object> newTask, object newParameter, Action<object> newPostFunction, KSPTSThreadingGroups group)
        {
            KSPTSParametrizedTask _paraAction = new KSPTSParametrizedTask(newTask, newParameter, newPostFunction);
            lock(locker)
            {
                _tasks.Enqueue(_paraAction, group);
                Monitor.Pulse(locker);
            }
        }

        internal void SetUrgent(KSPTSThreadingGroups urgentGroup)
        {
            lock (locker)
            {
                _tasks.SetUrgent(urgentGroup);
            }
        }

        internal bool IsBusy()
        {
            bool value = false;
            lock(locker)
            {
                value = busy;
            }
            return value;
        }

        private void DoTasks()
        {
            while(true)
            {
                KSPTSParametrizedTask currentTask = null;

                lock (locker)
                {
                    busy = false;
                    while (!_tasks.hasTasks)
                    {
                        Monitor.Wait(locker);
                    }
                    busy = true;
                    currentTask = _tasks.Dequeue();
                }

                if (currentTask.action == null)
                    continue;

                object postFuncParam = currentTask.action(currentTask.parameter);

                KSPTSThreadController.instance.EnqueuePostFunction(currentTask.postFunction, postFuncParam);
            }
        }
    }
}
