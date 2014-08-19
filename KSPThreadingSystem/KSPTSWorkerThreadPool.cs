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
        private Queue<KSPTSParametrizedFunc> _tasks = new Queue<KSPTSParametrizedFunc>();
        private readonly object locker = new object();

        internal bool busy;

        internal KSPTSWorkerThreadPool() : this(Environment.ProcessorCount) { }

        internal KSPTSWorkerThreadPool(int numThreads)
        {
            _threads = new Thread[numThreads];

            for (int i = 0; i < numThreads; i++)
                (_threads[i] = new Thread(DoTasks)).Start();
        }

        internal void EnqueueNewTask(Func<object, object> newTask, object newParameter)
        {
            KSPTSParametrizedFunc _paraAction = new KSPTSParametrizedFunc(newTask, newParameter);
            lock(locker)
            {
                _tasks.Enqueue(_paraAction);
                Monitor.Pulse(locker);
            }
        }

        private void DoTasks()
        {
            while(true)
            {
                KSPTSParametrizedFunc currentTask = null;

                lock (locker)
                {
                    busy = false;
                    while (_tasks.Count == 0)
                    {
                        Monitor.Wait(locker);
                    }
                    busy = true;
                    currentTask = _tasks.Dequeue();
                }

                if (currentTask.action == null)
                    return;

                currentTask.action(currentTask.parameter);
            }
        }
    }
}
