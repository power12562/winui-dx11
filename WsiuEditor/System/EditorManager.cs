using System;
using System.Collections.Generic;
using WsiuEditor.Editor;
using WsiuEngine.Core;

namespace WsiuEditor.System
{
    internal partial class EditorManager(Engine engine)
    {
        private readonly Engine _engine = engine;
        private readonly List<IEditor> _editors = [];
        private readonly Dictionary<Type, UInt64> _idCounter = [];

        public void CreateEditor<T>(Engine engine) where T : IEditor
        {
            if (editorProvider.TryGetValue(typeof(T), out var func))
            {
                IEditor iEditor = func(engine, GetEditorId(typeof(T)));
                if (iEditor is T editor)
                {
                    _editors.Add(editor);
                }
            }
        }

        public void CreateEditor(Type type, Engine engine)
        {
            if (editorProvider.TryGetValue(type, out var func))
            {
                IEditor iEditor = func(engine, GetEditorId(type));
                _editors.Add(iEditor);
            }
        }

        internal void DrawEditors()
        {
            foreach (IEditor editor in _editors)
            {
                editor.Draw();
            }
        }

        private UInt64 GetEditorId(Type type)
        {
            _idCounter.TryGetValue(type, out UInt64 id);
            _idCounter[type] = id + 1;
            return id;
        }
    }
}
