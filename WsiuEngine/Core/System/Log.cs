using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WsiuEngine.Extensions;

namespace WsiuEngine.Core.System
{
    public class Log
    {
        private static Log instance = null!;
        internal static void Initialize()
        {
            instance = new();
        }

        private readonly ConcurrentQueue<Entry> _entryQueue;
        private Log() 
        {
            _entryQueue = new();
            _loopTask = Task.Run(() => WriteLoop().Forget());
        }

        public enum Level
        { 
            Trace = 0,
            Debug = 1,
            Info = 2,
            Warning = 3,
            Error = 4,
            Fatal = 5 
        }

        public record Entry(

            DateTime Time,
            Level Level,
            string Message,
            string FilePath,
            string FileName,
            string MemberName,
            int LineNumber
        );

        public static string DisplayHeader(Entry logEntry)
        {
            return $"[{logEntry.Time:HH:mm:ss}] ({logEntry.Level}) {logEntry.Message}";
        }
        public static string DisplaySub(Entry logEntry)
        {
            return $"{logEntry.FileName}.{logEntry.MemberName}, Line: {logEntry.LineNumber} ({logEntry.FileName})"; 
        }

        public static event Action<Entry>? OnLogReceived;
        public static void Trace(string msg, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        {
            instance.Message(Level.Trace, msg, path, line, member);
        }
        public static void Debug(string msg, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        {
            instance.Message(Level.Debug, msg, path, line, member);
        }
        public static void Info(string msg, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        {
            instance.Message(Level.Info, msg, path, line, member);
        }
        public static void Warning(string msg, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        {
            instance.Message(Level.Warning, msg, path, line, member);
        }
        public static void Error(string msg, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        {
            instance.Message(Level.Error, msg, path, line, member);
        }
        public static void Fatal(string msg, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        {
            instance.Message(Level.Fatal, msg, path, line, member);
        }

        private void Message(Level level, string msg, string path, int line, string member)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            Entry entry = new(DateTime.Now, level, msg, path, fileName, member, line);
            _entryQueue.Enqueue(entry);
            TaskDispatcher.PostToDispatcher(() => OnLogReceived?.Invoke(entry));
        }

        private static string MakeLogStream(Entry entry)
        {
            return $"[{entry.Time:yyyy-MM-dd HH:mm:ss.fff}] " +
                   $"[{entry.Level,-5}] " +
                   $"[{entry.FileName}:{entry.LineNumber} @ {entry.MemberName}] " +
                   $"-> {entry.Message}";
        }

        public static string GetLogDirectory()
        {
            return Path.Combine(WindowService.AppLocalFolderPath, "Logs");
        }

        private volatile bool _flushEntries = false;
        private readonly Task _loopTask;
        private async Task WriteLoop()
        {
            string fileName = $"{DateTime.Now:yyyy-MM-dd_HHmmss}.log";
            string logDir = GetLogDirectory();
            Directory.CreateDirectory(logDir);
            string logPath = Path.Combine(logDir, fileName);
            using StreamWriter sw = new(logPath, append: true) { AutoFlush = true };
            while (true)
            {
                if (_entryQueue.TryDequeue(out var entry))
                {
                    await sw.WriteLineAsync(MakeLogStream(entry));
                }
                else
                {
                    await Task.Delay(10);
                }

                if (_flushEntries)
                    break;
            }

            while(_entryQueue.TryDequeue(out var entry))
            {
                await sw.WriteLineAsync(MakeLogStream(entry));
            }
            sw.Flush();
        }

        internal static void FlushEntries()
        {
            instance.InternalFlushEntries();
        }

        internal void InternalFlushEntries()
        {
            _flushEntries = true;
            _loopTask.Wait();
        }
    }
}
