using ABI.WsiuRenderer;
using System;
using System.Collections.Generic;
using System.Numerics;
using WsiuEditor.Interfaces;
using WsiuEditor.Editor.Base;
using WsiuEngine.Collections;
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
            TestDraw();
            TestDraw2();
        }

        private readonly TestClassDraw _testClass = new();
        private void TestDraw()
        {
            _imguiContext.TreeNodeEx("Test Draw", ImGuiTreeNodeFlags.None);

            _imguiContext.Text("Fields");
            ReflectionObject.DrawFields(_imguiContext, _testClass);
            _imguiContext.Separator();
            _imguiContext.Text("Methods");
            _imguiContext.PushStyleColor(ImGuiCol.Text, 0.4f, 0.7f, 1.0f, 1.0f);
            ReflectionObject.DrawMethods(_imguiContext, _testClass);
            _imguiContext.PopStyleColor();

            _imguiContext.TreePop();
        }

        private readonly TestClassDraw2 _testClass2 = new();
        private void TestDraw2()
        {
            _imguiContext.TreeNodeEx("Test Draw2", ImGuiTreeNodeFlags.None);

            _imguiContext.Text("Fields");
            ReflectionObject.DrawFields(_imguiContext, _testClass2);
            _imguiContext.Separator();
            _imguiContext.Text("Methods");
            _imguiContext.PushStyleColor(ImGuiCol.Text, 0.4f, 0.7f, 1.0f, 1.0f);
            ReflectionObject.DrawMethods(_imguiContext, _testClass2);
            _imguiContext.PopStyleColor();

            _imguiContext.TreePop();
        }

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

            [SerializeMethod]
            public int AddFoo(int foo, int tempParameter, int c)
            {
                return foo + tempParameter + c;
            }
        }

        class TestClassDraw2
        {
            [MultilineStringField]
            [ReadOnlyField]
            public string TestSerialize = "";

            public IdProvider IdProvider = new();
            private readonly List<UInt64> _idList = [];

            [SerializeMethod]
            public void Serialize()
            {
                TestSerialize = ReflectionObject.SerializeToJson(this);
            }

            [SerializeMethod]
            public void Deserailize()
            {
                ReflectionObject.DeserializeFromJson(this, TestSerialize);
            }

            [SerializeMethod] 
            public void GenerateId()
            {
                _idList.Add(IdProvider.Generate());
            }

            [SerializeMethod]
            public void ReleaseId()
            {
                IdProvider.Release(_idList[^1]);
                _idList.RemoveAt(_idList.Count - 1);
            }

            [SerializeMethod]
            public void ClearIdProvider()
            {
                IdProvider = new();
            }
        }
    }
}
