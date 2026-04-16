using System;
using System.Threading.Tasks;
using WsiuEngine.Core.System;

namespace WsiuEngine.Extensions
{
    public static class TaskExtensions
    {
        public static Task Forget(this Task task)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    Log.Error($"[Task Error] {ex.Message}");
                }
            });
        }

        public static Task SubmitToEngine(this Task task, Action<Task> handle)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await task;
                    TaskDispatcher.PostToDispatcher(() =>
                    {
                        handle(task);
                    });
                }
                catch (Exception ex)
                {
                    Log.Error($"[Task Error] {ex.Message}");
                }
            });
        }

        public static void SubmitToEngine<TResult>(this Task<TResult> task, Action<TResult> handle)
        {
            Task.Run(async () =>
            {
                try
                {
                    TResult result = await task;
                    TaskDispatcher.PostToDispatcher(() => handle(result));
                }
                catch (Exception ex)
                {
                    Log.Error($"[Task Error] {ex.Message}");
                }
            });
        }
    }
}
