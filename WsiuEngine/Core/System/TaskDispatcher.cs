using System;
using System.Collections.Concurrent;

namespace WsiuEngine.Core.System
{
    internal sealed class TaskDispatcher
    {
        private static TaskDispatcher instance = null!;
        internal static void Initialize()
        {
            instance = new TaskDispatcher();
        }

        private readonly ConcurrentQueue<Action> _taskQueue;
        private TaskDispatcher()
        {
            _taskQueue = [];
        }

        internal static void DispatchPendingTasks()
        {
            instance.InternalDispatchPendingTasks();
        }
        internal void InternalDispatchPendingTasks()
        {
            while (_taskQueue.TryDequeue(out Action? action) == true)
            {
                action();
            }
        }

        internal static void PostToDispatcher(Action action)
        {
            instance.InternalPostToDispatcher(action);
        }
        internal void InternalPostToDispatcher(Action action)
        {
            _taskQueue.Enqueue(action);
        }
    }
}
