using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Graphics;
using WsiuEditor.Editor;
using WsiuEditor.System;
using WsiuEngine.Core;

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

            CompositionTarget.Rendering += (sender, args) => EditorLoop();
            if (Content is FrameworkElement frameworkElement)
            {
                frameworkElement.Loaded += (obj, eventArgs) => SetWindowToFullWorkArea();
            }    
        }

        private void EditorLoop()
        {
            _editorManager.Draw();
            _engine.Update();
        }

        private void SetWindowToFullWorkArea()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            if (appWindow != null)
            {
                DisplayArea displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
                RectInt32 workArea = displayArea.WorkArea;
                appWindow.MoveAndResize(new RectInt32(
                    workArea.X,
                    workArea.Y,
                    workArea.Width,
                    workArea.Height
                ));
            }
        }
    }
}
