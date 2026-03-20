using System;
using System.Collections.Generic;
using WsiuEditor.Editor;
using WsiuEngine.Core;
using WsiuRenderer;

namespace WsiuEditor.System
{
    public partial class EditorManager
    {
        public EditorManager(Engine engine)
        {
            _engine = engine;
            _imguiContext = new(engine.EngineCore);
            _imguiContext.InitializeCommands("Editor Manager Commands");
        }

        private readonly Engine _engine;
        private readonly ImguiContext _imguiContext;
        private readonly List<IEditor> _transientEditors = [];
        private readonly List<IEditor> _singletonEditors = [];
        private readonly Dictionary<Type, IEditor> _singletonEditorInstance = [];
        private readonly Dictionary<Type, UInt64> _editorIdCounter = [];
        private bool _cleanupEditors = false;
        private void CleanUpEditors() 
        { 
            _cleanupEditors = true; 
        }

        public void CreateTransientEditor<T>() where T : IEditor
        {
            Type type = typeof(T);
            CreateTransientEditor(type);
        }

        public void CreateTransientEditor(Type type)
        {
            if (EditorManager.transientProvider.TryGetValue(type, out var provider))
            {
                IEditor iEditor = provider(_engine, AddEditorId(type));
                _transientEditors.Add(iEditor);
                iEditor.SetDisableCallback(CleanUpEditors);
            }
        }

        public void ActiveStaticEditor<T>() where T : IEditor
        {
            Type type = typeof(T);
            ActiveSingletonEditor(type);
        }

        public void ActiveSingletonEditor(Type type) 
        {
            if (_singletonEditorInstance.TryGetValue(type, out var singletonEditor))
            {
                singletonEditor.Active = true; 
                return;
            }

            if (EditorManager.singletonProvider.TryGetValue(type, out var provider))
            {
                IEditor iEditor = provider(_engine);
                _singletonEditors.Add(iEditor);
                _singletonEditorInstance.Add(type, iEditor);
            }
        }

        internal void Draw()
        {
            DrawMainMenuBar();
            DrawEditors();
        }

        private void DrawEditors()
        {
            foreach (IEditor editor in _singletonEditors)
            {
                editor.Draw();
            }

            foreach (IEditor editor in _transientEditors)
            {
                editor.Draw();
            }

            if (_cleanupEditors)
            {
                for (int i = _transientEditors.Count - 1; i >= 0; i--)
                {
                    IEditor editor = _transientEditors[i];
                    if(editor.Active == false)
                    {
                        _transientEditors.RemoveAt(i);
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
