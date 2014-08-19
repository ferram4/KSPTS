using System;
using System.Collections.Generic;
using System.Threading;

namespace KSPThreadingSystem
{
    /// <summary>
    /// This class handles all the worker threads used by KSPTS.  Actions are queued up, and then are given to workers as they become available
    /// </summary>
    internal class KSPTSWorkerThreadPool
    {
        private Thread[] _threads;
        private Queue<Action> _tasks = new Queue<Action>();
        private readonly object locker = new object();

        internal bool busy;

        internal KSPTSWorkerThreadPool() : this(Environment.ProcessorCount) { }

        internal KSPTSWorkerThreadPool(int numThreads)
        {
            _threads = new Thread[numThreads];

            for (int i = 0; i < numThreads; i++)
                (_threads[i] = new Thread(DoTasks)).Start();
        }

        internal void EnqueueNewTask(Action newTask)
        {
            lock(locker)
            {
                _tasks.Enqueue(newTask);
                Monitor.Pulse(locker);
            }
        }

        private void DoTasks()
        {
            while(true)
            {
                Action currentTask = null;

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

                if (currentTask == null)
                    return;

                currentTask();
            }
        }
    }
}
