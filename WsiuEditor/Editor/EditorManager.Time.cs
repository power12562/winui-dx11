using WsiuEngine.Core;
using WsiuEngine.Core.System;
using WsiuRenderer;

namespace WsiuEditor.Editor
{
    internal partial class EditorManager
    {
        private readonly ImguiContext _timeDebugUI;
        private void DrawTimeDebug()
        {
            ImguiContext.SettingFloat(0.01f, 0, 0, "%.3f", ImGuiSliderFlags.None);
            ImguiContext.SettingDouble(0.01f, 0, 0, "%.3f", ImGuiSliderFlags.None);
            ReflectedObject.DrawFields(_timeDebugUI, Engine.Time);
        }
    }
}
