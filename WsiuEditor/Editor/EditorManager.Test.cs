using System;
using System.Numerics;
using WsiuEngine.Core.System;
using WsiuRenderer;

namespace WsiuEditor.Editor
{
    internal partial class EditorManager
    {

        private readonly ImguiContext _test;
        private void DrawTestDebug()
        {
            _test.TreeNodeEx("Test Methods", ImGuiTreeNodeFlags.None);
            _test.PushStyleColor(ImGuiCol.Text, 0.4f, 0.7f, 1.0f, 1.0f);
            ReflectedObject.DrawMethods(_test, _testClass.TestVector);
            _test.PopStyleColor();
            _test.TreePop();

            ReflectedObject.DrawFields(_test, _testClass);
        }

        private TestClassDraw _testClass = new();
        class TestVectorDraw(TestClassDraw refer)
        {
            [SerializeField]
            private TestClassDraw testField = refer;

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

