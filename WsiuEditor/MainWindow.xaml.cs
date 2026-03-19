using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Numerics;
using WsiuEngine.Core;
using WsiuEngine.Core.System;
using WsiuEngine.Extensions;
using WsiuRenderer;

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
            _testDebugUI.PushStyleColor(ImGuiCol.Text, 0.4f, 0.7f, 1.0f, 1.0f);
            ReflectedObject.DrawMethods(_testDebugUI, test.TestVector);
            _testDebugUI.PopStyleColor();
            _testDebugUI.TreePop();

            ReflectedObject.DrawFields(_testDebugUI, test);
        }

        class TestVectorDraw(TestClassDraw refer)
        {
            [SerializeField]
            private TestClassDraw testField = refer;

            public Vector2 Vector2 = new();
            public Vector3 Vector3 = new();
            public Vector4 Vector4 = new();

            public Vector2 AddVector2()
            {
                Vector2 += new Vector2(1, 1);
                return Vector2;
            }

            public Vector2 AddVector2(Vector2 target)
            {
                Vector2 += target;
                return Vector2;
            }

            [SerializeMethod]
            private Vector2 subVector2(Vector2 target)
            {
                Vector2 -= target;
                return Vector2;
            }
        }

        class TestClassDraw
        {
            public TestClassDraw()
            {
                TestVector = new(this);
            }

            public readonly TestVectorDraw TestVector;
            public float TestFloat = 1.1f;
            public Int16 TestInt16 = -16;
            public Int32 TestInt32 = -32;
            public Int64 TestInt64 = -64;
            public UInt16 TestUInt16 = 16;
            public UInt32 TestUInt32 = 32;
            public UInt64 TestUInt64 = 64;
        }
    }
}
