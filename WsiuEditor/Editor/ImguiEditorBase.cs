using Microsoft.UI.Xaml.Documents;
using System;
using WsiuEngine.Core;
using WsiuRenderer;

namespace WsiuEditor.Editor
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
                _imguiContext.SetTitle(GetImguiTitle());
            }
        }
        private string _name = string.Empty;

        public bool Active 
        {
            get => _imguiContext.GetActive();
            set => _imguiContext.SetActive(value);
        }
        private readonly string _typeName;
        protected string GetImguiTitle() { return $"{_name}###{_typeName}{ID}"; }

        public abstract void Draw();

        public void SetDisableCallback(Action callback)
        {
            _engineCore.EditorDisableCallback(_imguiContext.GetWindowID(), () => callback());
        }

        protected readonly ImguiContext _imguiContext;
        protected readonly EngineCore _engineCore;
        protected ImguiEditorBase(Engine engine, UInt64 id)
        {
            _engineCore = engine.EngineCore;
            _imguiContext = new(_engineCore);
            _typeName = GetType().Name;
            _id = id;
        }
    }
}
