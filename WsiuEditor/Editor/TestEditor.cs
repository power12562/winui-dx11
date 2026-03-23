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
            _imguiContext.Text("Fields");
            ReflectionObject.DrawFields(_imguiContext, _testClass);
            _imguiContext.Separator();
            _imguiContext.Text("Methods");
            _imguiContext.PushStyleColor(ImGuiCol.Text, 0.4f, 0.7f, 1.0f, 1.0f);
            ReflectionObject.DrawMethods(_imguiContext, _testClass);
            _imguiContext.PopStyleColor();
        }

        private readonly TestClassDraw _testClass = new();
        class TestClassDraw
        {
            public float TestFloat = 1.1f;
            public Int16 TestInt16 = -16;
            public Int32 TestInt32 = -32;
            public Int64 TestInt64 = -64;
            public UInt16 TestUInt16 = 16;
            public UInt32 TestUInt32 = 32;
            public UInt64 TestUInt64 = 64;
            public Vector2 TestVector2 = new();
            public Vector3 TestVector3 = new();
            public Vector4 TestVector4 = new();

            [HideInInspector]
            public string TestString = "";

            [MultilineStringField(Height = 100f)]
            public string TestSerialize => _testSerialize;
            private string _testSerialize = "";


            [SerializeMethod]
            public void Serialize()
            {
                _testSerialize = ReflectionObject.SerializeToJson(this);
            }
            [SerializeMethod]
            public void Deserialize()
            {
                ReflectionObject.DeserializeFromJson(this, _testSerialize);
            }
        }
    }
}
