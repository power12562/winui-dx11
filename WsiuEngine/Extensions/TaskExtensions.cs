using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WsiuEngine.Core.System;

namespace WsiuEngine.Extensions
{
    public static class TaskExtensions
    {
        public static void Forget(this Task task)
        {
            Task.Run(async () =>
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    //TODO: 이후 엔진 전용 로그로 교체해야함.
                    Debug.WriteLine($"[Task Error] {ex.Message}");
                    Debug.WriteLine(ex.StackTrace);
                    if (Debugger.IsAttached)
                        Debugger.Break();
                }
            });
        }

        public static void SubmitToEngine(this Task task, Action<Task> handle)
        {
            Task.Run(async () =>
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
                    //TODO: 이후 엔진 전용 로그로 교체해야함.
                    Debug.WriteLine($"[Task Error] {ex.Message}");
                    Debug.WriteLine(ex.StackTrace);
                    if (Debugger.IsAttached)
                        Debugger.Break();
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
                    //TODO: 이후 엔진 전용 로그로 교체해야함.
                    Debug.WriteLine($"[Task Error] {ex.Message}");
                    Debug.WriteLine(ex.StackTrace);
                    if (Debugger.IsAttached)
                        Debugger.Break();
                }
            });
        }
    }
}
