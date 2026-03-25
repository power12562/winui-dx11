using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using WsiuEditor.Editor;
using WsiuEngine.Collections;
using WsiuEngine.Core;
using WsiuEngine.Core.System;
using WsiuRenderer;

namespace WsiuEditor.System
{
    public partial class EditorManager : ReflectionObject.ISerializationCallback
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
        private bool _cleanupEditors = false;
        private Dictionary<Type, IdProvider> _editorIdProvider = [];

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
            CreateTransientEditorWithId(type, GenerateEditorId(type));
        }

        private void CreateTransientEditorWithId(Type type, UInt64 id)
        {
            if (EditorManager.transientProvider.TryGetValue(type, out var provider))
            {
                IEditor iEditor = provider(_engine, id);
                _transientEditors.Add(iEditor);
                iEditor.SetDisableCallback(CleanUpEditors);
            }
        }

        public void ActiveSingletonEditor<T>() where T : IEditor
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
                        ReleaseEditorId(editor.GetType(), editor.ID);
                        _transientEditors.RemoveAt(i);
                    }
                }
                _cleanupEditors = false;
            }
        }

        private UInt64 GenerateEditorId(Type type)
        {
            if (_editorIdProvider.TryGetValue(type, out IdProvider? provider) == false)
            {
                provider = new();
                _editorIdProvider.Add(type, provider);
            }          
            return provider.Generate();
        }

        private void ReleaseEditorId(Type type, UInt64 id)
        {
            if (_editorIdProvider.TryGetValue(type, out IdProvider? provider) == false)
                return;

            provider.Release(id);
        }
    }
}
