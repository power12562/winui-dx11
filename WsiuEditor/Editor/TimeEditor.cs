using System;
using WsiuEngine.Core;
using WsiuEngine.Core.System;
using WsiuRenderer;

namespace WsiuEditor.Editor
{
    internal class TimeEditor : EditorBase
    {
        public TimeEditor(Engine engine, UInt64 id) : base(engine, id) 
        {       
            _imguiContext.InitializeWindowClosable("Time");
            Name = "Time";
        }

        public override void Draw()
        {
            ImguiContext.SettingFloat(0.01f, 0, 0, "%.3f", ImGuiSliderFlags.None);
            ImguiContext.SettingDouble(0.01f, 0, 0, "%.3f", ImGuiSliderFlags.None);
            ReflectedObject.DrawFields(_imguiContext, Engine.Time);
        }

    }
}
