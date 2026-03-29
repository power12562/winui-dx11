using WsiuEditor.Editor.Base;
using WsiuEditor.Interfaces;
using WsiuEngine.Core;
using WsiuEngine.Core.System;

namespace WsiuEditor.Editor
{
    [SingletonEditor]
    internal sealed partial class LogEditor : ImguiEditorBase
    {
        private const string editorName = "Log";
        public LogEditor(Engine engine, ulong id) : base(engine, id)
        {
            _imguiContext.InitializeWindowClosable(editorName);
            Name = editorName;
            Log.OnLogReceived += OnLogReceived;
        }

        public override void Draw()
        {

        }

        private void OnLogReceived(Log.Entry log)
        {

        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            if (disposing)
            {
                Log.OnLogReceived -= OnLogReceived;
            }
            base.Dispose(disposing);
        }
    }
}
