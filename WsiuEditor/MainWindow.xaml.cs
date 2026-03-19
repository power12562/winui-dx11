using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WsiuEngine.Core;
using WsiuEditor.System;
using WsiuEditor.Editor;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WsiuEditor
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly Engine _engine;
        private readonly EditorManager _editorManager;

        public MainWindow()
        {
            InitializeComponent();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            _engine = new Engine(hwnd, EnginePanel);

            EditorManager.RegisterEditors();
            _editorManager = new(_engine);
            _editorManager.CreateEditor<TestEditor>();
            _editorManager.CreateEditor<TimeEditor>();

            CompositionTarget.Rendering += (sender, args) => EditorLoop();
        }

        private void EditorLoop()
        {
            _editorManager.DrawEditors();
            _engine.Update();
        }
    }
}
