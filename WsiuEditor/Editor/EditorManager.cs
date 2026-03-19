using WsiuEngine.Core;
using WsiuEngine.Extensions;

namespace WsiuEditor.Editor
{
    internal partial class EditorManager
    {
        public EditorManager(Engine engine)
        {
            _timeDebugUI = ImguiContextExtensions.CreateImguiContext(engine.EngineCore, "Time", ContextType.Default);
            _test = ImguiContextExtensions.CreateImguiContext(engine.EngineCore, "Debug", ContextType.Default);
        }

        internal void DrawEditors()
        {
            DrawTimeDebug();
            DrawTestDebug();
        }
    }
}
