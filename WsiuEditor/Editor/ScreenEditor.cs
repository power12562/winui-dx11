using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WsiuEditor.Editor.Base;
using WsiuEditor.Interfaces;
using WsiuEngine.Core;
using WsiuEngine.Core.System;
using WsiuRenderer;

namespace WsiuEditor.Editor
{
    [SingletonEditor]
    public class ScreenEditor : ImguiEditorBase
    {
        private const string editorName = "Screen";

        public ScreenEditor(Engine engine, ulong id) : base(engine, id)
        {
            _imguiContext.InitializeWindowClosable(editorName);
            Name = editorName;
        }

        public override void Draw()
        {
            ImguiContext.SettingInt32(1.0f, 0, 0, "%d", ImGuiSliderFlags.None);
            ReflectionObject.DrawStaticFields<Screen>(_imguiContext);
        }
    }
}
