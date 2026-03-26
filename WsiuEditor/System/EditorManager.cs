using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using WsiuEditor.Interfaces;
using WsiuEngine.Collections;
using WsiuEngine.Core;
using WsiuEngine.Core.System;
using WsiuRenderer;

namespace WsiuEditor.System
{
    public partial class EditorManager : ReflectionObject.ISerializationCallback
    {
        private static EditorManager instance = null!;
        internal static void Initialize(Engine engine)
        {
            instance = new EditorManager(engine);
        }

        private EditorManager(Engine engine)
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

        public static void CreateTransientEditor<TEditor>() where TEditor : IEditor
        {
            instance.InternalCreateTransientEditor<TEditor>();
        }

        internal void InternalCreateTransientEditor<TEditor>() where TEditor : IEditor
        {
            Type type = typeof(TEditor);
            InternalCreateTransientEditor(type);
        }

        public static void CreateTransientEditor(Type type)
        {
            instance.InternalCreateTransientEditor(type);
        }

        internal void InternalCreateTransientEditor(Type type)
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

        public static void ActiveSingletonEditor<TEditor>() where TEditor: IEditor
        {
            instance.InternalActiveSingletonEditor<TEditor>();
        }

        internal void InternalActiveSingletonEditor<TEditor>() where TEditor : IEditor
        {
            Type type = typeof(TEditor);
            InternalActiveSingletonEditor(type);
        }

        public static void ActiveSingletonEditor(Type type)
        {
            instance.InternalActiveSingletonEditor(type);
        }

        internal void InternalActiveSingletonEditor(Type type) 
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

        internal static void Update()
        {
            instance.InternalDraw();
        }

        internal void InternalDraw()
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
