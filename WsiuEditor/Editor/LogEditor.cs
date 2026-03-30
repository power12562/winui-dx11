using System;
using System.Collections.Generic;
using WsiuEditor.Editor.Base;
using WsiuEditor.Interfaces;
using WsiuEngine.Core;
using WsiuEngine.Core.System;
using WsiuEngine.Core.System.Keyboard;
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

        private readonly List<string> _displayList = [];
        private readonly List<string> _logfilePathList = [];
        private readonly string _testLog = "";
        public override void Draw()
        {
            static void Message(string message)
            {
                Log.Debug(message);
            }
            _imguiContext.InputText("Debug", _testLog, Message);
            _imguiContext.BeginChild("LogConsoleRegion", new(0, 0), ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar);
            void ClickItem(Int32 index)
            {
                if (InputSystem.GetKeyState(KeyCode.LeftControl) == KeyState.Held)
                {
                    Log.Debug($"경로: {_logfilePathList[index]}");
                }
            }
            _imguiContext.DrawTextListClipper(_displayList, "어쩔툴팁", 2, ClickItem);
            _imguiContext.EndChild();
        }

        private void OnLogReceived(Log.Entry log)
        {
            _displayList.Add($"{Log.DisplayHeader(log)}\n{Log.DisplaySub(log)}");
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
    }
}
