using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WsiuEngine.Extensions
{
    public static class TaskExtensions
    {
        public static void Forget(this Task task)
        {
            if (task == null)
                return;

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
                    if(Debugger.IsAttached) 
                        Debugger.Break();
                }
            });
        }

    }
}
