using ABI.WsiuRenderer;
using System;
using System.Numerics;
using WsiuEngine.Core;
using WsiuEngine.Core.System;
using WsiuRenderer;

namespace WsiuEditor.Editor
{
    [SingletonEditor]
    internal class TestEditor : ImguiEditorBase
    {
        public TestEditor(Engine engine, UInt64 id) : base(engine, id)
        {
            _imguiContext.InitializeWindowClosable("Test");
            Name = "Test";
        }

        public override void Draw()
        {
            _imguiContext.TreeNodeEx("TestVectorDraw Methods", ImGuiTreeNodeFlags.None);
            _imguiContext.PushStyleColor(ImGuiCol.Text, 0.4f, 0.7f, 1.0f, 1.0f);
            ReflectedObject.DrawMethods(_imguiContext, _imguiContextClass.TestVector);
            _imguiContext.PopStyleColor();
            _imguiContext.TreePop();

            _imguiContext.TreeNodeEx("TestClassDraw Fields##1", ImGuiTreeNodeFlags.None);
            ReflectedObject.DrawFields(_imguiContext, _imguiContextClass);
            _imguiContext.TreePop();
            _imguiContext.TreeNodeEx("TestClassDraw Fields##2", ImGuiTreeNodeFlags.None);
            ReflectedObject.DrawFields(_imguiContext, _imguiContextClass);
            _imguiContext.TreePop();
        }

        private readonly TestClassDraw _imguiContextClass = new();
        class TestVectorDraw(TestClassDraw refer)
        {
            [SerializeField]
            private readonly TestClassDraw testField = refer;

            public Vector2 Vector2 = new();
            public Vector3 Vector3 = new();
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

            [SerializeMethod]
            private Vector2 SubVector2(Vector2 target)
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
