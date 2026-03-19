using System;
using System.Collections.Generic;
using WsiuEditor.Editor;
using WsiuEngine.Core;
using WsiuRenderer;

namespace WsiuEditor.System
{
    internal partial class EditorManager(Engine engine)
    {
        private readonly Engine _engine = engine;
        private readonly ImguiContext _imguiContext = new(engine.EngineCore);
        private readonly List<IEditor> _editors = [];
        private readonly Dictionary<Type, UInt64> _editorIdCounter = [];

        public void CreateEditor<T>() where T : IEditor
        {
            if (editorProvider.TryGetValue(typeof(T), out var func))
            {
                IEditor iEditor = func(_engine, AddEditorId(typeof(T)));
                if (iEditor is T editor)
                {
                    _editors.Add(editor);
                }
            }
        }

        public void CreateEditor(Type type)
        {
            if (editorProvider.TryGetValue(type, out var func))
            {
                IEditor iEditor = func(_engine, AddEditorId(type));
                _editors.Add(iEditor);
            }
        }

        internal void DrawEditors()
        {
            foreach (IEditor editor in _editors)
            {
                DrawMainMenuBar();
                editor.Draw();
            }
        }

        private UInt64 AddEditorId(Type type)
        {
            _editorIdCounter.TryGetValue(type, out UInt64 id);
            _editorIdCounter[type] = id + 1;
            return id;
        }
    }
}
