using System;
using WsiuEditor.Interfaces;
using WsiuEngine.Core;
using WsiuRenderer;

namespace WsiuEditor.Editor.Base
{
    public abstract class ImguiEditorBase : IEditor
    {
        public UInt64 ID
        {
            get => _id;
        }
        private readonly UInt64 _id;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                _imguiTitle = $"{_name}###{_imguiId}";
                _imguiContext.SetTitle(_imguiTitle);
            }
        }
        private string _name;

        public bool Active
        {
            get => _imguiContext.GetActive();
            set => _imguiContext.SetActive(value);
        }


        protected string ImguiTitle => _imguiTitle;
        private string _imguiTitle;

        protected string ImguiId => _imguiId;
        private readonly string _imguiId;

        public abstract void Draw();

        public void SetDisableCallback(Action callback)
        {
            _engineCore.EditorDisableCallback(_imguiContext.GetWindowID(), () => callback());
        }

        protected readonly ImguiContext _imguiContext;
        protected readonly EngineCore _engineCore;
        private readonly string _typeName;
        protected ImguiEditorBase(Engine engine, UInt64 id)
        {
            _engineCore = engine.EngineCore;
            _imguiContext = new(_engineCore);
            _typeName = GetType().Name;
            _id = id;
            _imguiId = $"{_typeName}{ID}";
            _name = "Please set the editor name after initialization.";
            _imguiTitle = $"{_name}###{_imguiId}";
        }
    }
}
