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
        private readonly ImguiContext _testDebugUI;

        public MainWindow()
        {
            InitializeComponent();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            _engine = new Engine(hwnd, EnginePanel);

            _timeDebugUI = ImguiContextExtensions.CreateImguiContext(_engine.EngineCore, "Time", ContextType.Default);
            _testDebugUI = ImguiContextExtensions.CreateImguiContext(_engine.EngineCore, "Debug", ContextType.Default);
            CompositionTarget.Rendering += (sender, args) => EditorLoop();
        }

        private void EditorLoop()
        {
            TimeDraw();
            TestDraw();
            _engine.Update();
        }

        private void TimeDraw()
        {
            ImguiContext.SettingFloat(0.01f, 0, 0, "%.3f", ImGuiSliderFlags.None);
            ImguiContext.SettingDouble(0.01f, 0, 0, "%.3f", ImGuiSliderFlags.None);
            ReflectedObject.DrawFields(_timeDebugUI, Engine.Time);
        }

        private TestClassDraw test = new();
        void TestDraw()
        {
            _testDebugUI.TreeNodeEx("Test Methods", ImGuiTreeNodeFlags.None);
            ReflectedObject.DrawMethods(_testDebugUI, test.TestVector);
            _testDebugUI.TreePop();

            ReflectedObject.DrawFields(_testDebugUI, test);
        }

        class TestVectorDraw(TestClassDraw refer)
        {
            [SerializeField]
            TestClassDraw testField = refer;

            [SerializeField]
            public Vector2 Vector2 = new();

            [SerializeField]
            public Vector3 Vector3 = new();

            [SerializeField]
            public Vector4 Vector4 = new();

            [SerializeMethod]
            public Vector2 AddVector2()
            {
                Vector2 += new Vector2(1, 1);
                return Vector2;
            }

            [SerializeMethod]
            public Vector2 AddVector2(Vector2 target)
            {
                Vector2 += target;
                return Vector2;
            }
        }

        class TestClassDraw
        {
            public TestClassDraw()
            {
                TestVector = new(this);
            }

            [SerializeField]
            public readonly TestVectorDraw TestVector;

            [SerializeField]
            public float TestFloat = 1.1f;
        }
    }
}
