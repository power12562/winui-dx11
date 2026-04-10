using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using WsiuEditor.Editor.Base;
using WsiuEditor.Interfaces;
using WsiuEngine.Core;
using WsiuEngine.Core.System;
using WsiuEngine.Core.System.Keyboard;
using WsiuEngine.Extensions;
using WsiuRenderer;

namespace WsiuEditor.Editor
{
    [SingletonEditor]
    internal sealed partial class LogEditor : ImguiEditorBase
    {
        private const string editorName = "Log";
        public LogEditor(Engine engine, ulong id) : base(engine, id)
        {
            _imguiContext.InitializeWindowClosable(editorName);
            Name = editorName;
            Log.OnLogReceived += OnLogReceived;
        }
        private readonly List<Log.Level> _levelList = [];
        private readonly List<string> _displayList = [];
        private readonly List<string> _logfilePathList = [];
        public override void Draw()
        {
            void TestLogs()
            {
                foreach (Log.Level lv in Enum.GetValues<Log.Level>())
                {
                    Log.Message(lv, lv.ToString());
                }
            }

            void ClickItem(Int32 index)
            {
                if (InputSystem.GetKeyState(KeyCode.LeftControl) == KeyState.Held)
                {
                    Task.Run(() =>
                    {
                        string filePath = _logfilePathList[index];
                        if (File.Exists(filePath))
                        {
                            ProcessStartInfo psi = new()
                            {
                                FileName = filePath,
                                UseShellExecute = true,
                                Verb = "open"
                            };
                            Process.Start(psi);
                        }
                    }).Forget();
                }
            }

            void PushColor(Int32 index)
            {
                Log.Level lv = _levelList[index];
                Vector4 color = LogEditor.GetLevelColor(lv);
                ImguiContext.ImmediatelyPushStyleColor(ImGuiCol.Text, color.X, color.Y, color.Z, color.W);
            }

            static void PopColor(Int32 index)
            {
                ImguiContext.ImmediatelyPopStyleColor();
            }

            _imguiContext.Button("Test", TestLogs);
            _imguiContext.Button("Clear", Clear);
            _imguiContext.BeginChild("LogConsoleRegion", new(0, 0), ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar);
            _imguiContext.DrawTextListClipper(_displayList, "[Ctrl + Click] to open file.", 2, ClickItem, PushColor, PopColor);
            _imguiContext.EndChild();
        }

        private void Clear()
        {
            _levelList.Clear();
            _displayList.Clear();
            _logfilePathList.Clear();
        }

        private void OnLogReceived(Log.Entry log)
        {
            string key = $"{Log.DisplayHeader(log)}\n{Log.DisplaySub(log)}";
            _levelList.Add(log.Level);
            _displayList.Add(key);
            _logfilePathList.Add(log.FilePath);
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            if (disposing)
            {
                Log.OnLogReceived -= OnLogReceived;
            }
            base.Dispose(disposing);
        }

        private static Vector4 GetLevelColor(Log.Level level)
        {
            return level switch
            {
                Log.Level.Trace => new Vector4(0.50f, 0.50f, 0.50f, 1.0f),
                Log.Level.Debug => new Vector4(0.44f, 0.75f, 1.0f, 1.0f),
                Log.Level.Info => new Vector4(0.90f, 0.90f, 0.90f, 1.0f),
                Log.Level.Warning => new Vector4(1.00f, 0.84f, 0.00f, 1.0f),
                Log.Level.Error => new Vector4(1.00f, 0.33f, 0.33f, 1.0f),
                Log.Level.Fatal => new Vector4(1.00f, 0.00f, 1.00f, 1.0f),
                _ => new Vector4(1.00f, 1.00f, 1.00f, 1.0f),
            };
        }
    }
}
