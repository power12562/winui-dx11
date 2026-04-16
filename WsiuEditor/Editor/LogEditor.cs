using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        [Flags]
        public enum Filter
        {
            None = 0,
            Trace = 1 << 0,
            Debug = 1 << 1,
            Info = 1 << 2,
            Warning = 1 << 3,
            Error = 1 << 4,
            Fatal = 1 << 5
        }
        private static readonly IReadOnlyList<Filter> filters = Enum.GetValues<Filter>().ToList();
        private const string editorName = "Log";
        public LogEditor(Engine engine, ulong id) : base(engine, id)
        {
            _imguiContext.InitializeWindowClosable(editorName);
            Name = editorName;
            Log.OnLogReceived += OnLogReceived;
        }
        private readonly List<Log.Level> _displayLevelList = [];
        private readonly List<string> _displayLogList = [];
        private readonly List<string> _displayFilePathList = [];
        private readonly List<string> _renderLogList = [];
        private readonly List<int> _renderIndexList = [];
        private Filter _renderFilterFlags = Filter.None;
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
                        int i = _renderIndexList[index];
                        string filePath = _displayFilePathList[i];
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
                int i = _renderIndexList[index];
                Log.Level lv = _displayLevelList[i];
                Vector4 color = LogEditor.GetLevelColor(lv);
                ImguiContext.ImmediatelyPushStyleColor(ImGuiCol.Text, color.X, color.Y, color.Z, color.W);
            }

            static void PopColor(Int32 index)
            {
                ImguiContext.ImmediatelyPopStyleColor();
            }

            RefreshRenderLogList();
            _imguiContext.Button("Clear", Clear);
            _imguiContext.SameLine();
            _imguiContext.Button("Test", TestLogs);
            _imguiContext.BeginCombo("Filter", _renderFilterFlags.ToString());
            foreach (Filter value in filters)
            {
                if (value == Filter.None)
                    continue;

                bool hasFlag = _renderFilterFlags.HasFlag(value);
                _imguiContext.Selectable(value.ToString(), hasFlag, ImGuiSelectableFlags.DontClosePopups, () =>
                {
                    _renderFilterFlags ^= value;
                    _refresh = true;
                });
            }
            _imguiContext.Separator();
            _imguiContext.Selectable("Select All", _renderFilterFlags == (Filter)~0, ImGuiSelectableFlags.DontClosePopups, () =>
            {
                foreach (Filter value in filters)
                {
                    _renderFilterFlags |= value;
                }    
                _refresh = true;
            });
            _imguiContext.Selectable("Clear All", _renderFilterFlags == Filter.None, ImGuiSelectableFlags.DontClosePopups, () =>
            {
                _renderFilterFlags = Filter.None;
                _refresh = true;
            });
            _imguiContext.EndCombo();
            _imguiContext.BeginChild("LogConsoleRegion", new(0, 0), ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar);
            _imguiContext.DrawTextListClipper(_renderLogList, "[Ctrl + Click] to open file.", 2, ClickItem, PushColor, PopColor);
            _imguiContext.EndChild();
        }

        private void Clear()
        {
            _displayLevelList.Clear();
            _displayLogList.Clear();
            _displayFilePathList.Clear();
            _renderLogList.Clear();
            _renderIndexList.Clear();
        }

        private bool _refresh = false;
        private void RefreshRenderLogList()
        {
            if (_refresh == false) return;

            _renderLogList.Clear();
            _renderIndexList.Clear();
            for (int i = 0; i < _displayLogList.Count; i++)
            {
                AddRenderLog(_displayLogList[i], _displayLevelList[i], i);
            }
            _refresh = false;
        }

        private void AddRenderLog(string display, Log.Level logLevel, int originIndex)
        {
            if (_renderFilterFlags == Filter.None)
            {
                _renderLogList.Add(display);
                _renderIndexList.Add(originIndex);
            }
            else
            {
                int level = (int)logLevel;
                Filter filter = (Filter)(1 << level);
                if (_renderFilterFlags.HasFlag(filter))
                {
                    _renderLogList.Add(display);
                    _renderIndexList.Add(originIndex);
                }
            }
        }

        private void OnLogReceived(Log.Entry log)
        {
            string display = $"{Log.DisplayHeader(log)}\n{Log.DisplaySub(log)}";
            _displayLogList.Add(display);
            _displayLevelList.Add(log.Level);
            _displayFilePathList.Add(log.FilePath);
            AddRenderLog(display, log.Level, _displayLogList.Count - 1);
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
