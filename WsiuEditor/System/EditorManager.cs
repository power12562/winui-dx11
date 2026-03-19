using System;
using System.Collections.Generic;
using WsiuEditor.Editor;
using WsiuEngine.Core;
using WsiuRenderer;

namespace WsiuEditor.System
{
    internal partial class EditorManager
    {
        public EditorManager(Engine engine)
        {
            _engine = engine;
            _imguiContext = new(engine.EngineCore);
            _imguiContext.InitializeCommands("Editor Manager Commands");
        }

        private readonly Engine _engine;
        private readonly ImguiContext _imguiContext;
        private readonly List<IEditor> _editors = [];
        private readonly Dictionary<Type, UInt64> _editorIdCounter = [];
        private bool _cleanupEditors = false;
        private void CleanUpEditors() 
        { 
            _cleanupEditors = true; 
        }

        public void CreateEditor<T>() where T : IEditor
        {
            if (editorProvider.TryGetValue(typeof(T), out var func))
            {
                IEditor iEditor = func(_engine, AddEditorId(typeof(T)));
                if (iEditor is T editor)
                {
                    _editors.Add(editor);
                    editor.SetDisableCallback(CleanUpEditors);
                }
            }
        }

        public void CreateEditor(Type type)
        {
            if (editorProvider.TryGetValue(type, out var func))
            {
                IEditor iEditor = func(_engine, AddEditorId(type));
                _editors.Add(iEditor);
                iEditor.SetDisableCallback(CleanUpEditors);
            }
        }

        internal void Draw()
        {
            DrawMainMenuBar();
            DrawEditors();
        }

        private void DrawEditors()
        {
            foreach (IEditor editor in _editors)
            {
                editor.Draw();
            }

            if (_cleanupEditors)
            {
                for (int i = _editors.Count - 1; i >= 0; i--)
                {
                    IEditor editor = _editors[i];
                    if(editor.Active == false)
                    {
                        _editors.RemoveAt(i);
                    }
                }
                _cleanupEditors = false;
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
