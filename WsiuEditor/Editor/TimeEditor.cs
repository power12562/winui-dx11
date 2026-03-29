using System;
using WsiuEditor.Editor.Base;
using WsiuEditor.Interfaces;
using WsiuEngine.Core;
using WsiuEngine.Core.System;
using WsiuRenderer;

namespace WsiuEditor.Editor
{
    [SingletonEditor]
    internal sealed partial class TimeEditor : ImguiEditorBase
    {
        private const string editorName = "Time";

        public TimeEditor(Engine engine, UInt64 id) : base(engine, id)
        {
            _imguiContext.InitializeWindowClosable(editorName);
            Name = editorName;
        }

        public override void Draw()
        {
            ImguiContext.SettingFloat(0.01f, 0, 0, "%.3f", ImGuiSliderFlags.None);
            ImguiContext.SettingDouble(0.01f, 0, 0, "%.3f", ImGuiSliderFlags.None);
            ReflectionObject.DrawStaticFields<Time>(_imguiContext);
        }

    }
}
