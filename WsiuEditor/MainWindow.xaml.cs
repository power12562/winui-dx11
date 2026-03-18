using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WsiuEngine.Core;
using WsiuEngine.Core.System;
using WsiuEngine.Extensions;
using WsiuRenderer;
using System.Numerics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WsiuEditor
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly Engine _engine;
        private readonly ImguiContext _timeDebugUI;

        public MainWindow()
        {
            InitializeComponent();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            _engine = new Engine(hwnd, EnginePanel);

            _timeDebugUI = ImguiContextExtensions.CreateImguiContext(_engine.EngineCore, "Time", ContextType.Default);
            CompositionTarget.Rendering += (sender, args) => EditorLoop();
        }

        private void EditorLoop()
        {
            TimeDraw();
            _engine.Update();
        }

        private TestVectorDraw test = new();
        private void TimeDraw()
        {
            ImguiContext.SettingFloat(0.01f, 0, 0, "%.3f", ImGuiSliderFlags.None);
            ImguiContext.SettingDouble(0.01f, 0, 0, "%.3f", ImGuiSliderFlags.None);
            ReflectedObject.DragFields(_timeDebugUI, Engine.Time);
            ReflectedObject.DragFields(_timeDebugUI, test);
        }

        class TestVectorDraw
        {
            [SerializeField]
            public Vector2 Vector2 = new();

            [SerializeField]
            public Vector3 Vector3 = new();

            [SerializeField]
            public Vector4 Vector4 = new();
        }
    }
}
