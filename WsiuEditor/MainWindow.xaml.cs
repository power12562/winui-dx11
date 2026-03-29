using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WsiuEditor.System;
using WsiuEngine.Core;
using WsiuEngine.Core.System;

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

        public MainWindow()
        {
            InitializeComponent();

            _engine = new Engine(this, EnginePanel);

            EditorManager.RegisterEditorsProvider();
            EditorManager.Initialize(_engine);

            CompositionTarget.Rendering += (sender, args) => EditorLoop();
            if (Content is FrameworkElement frameworkElement)
            {
                frameworkElement.Loaded += OnWindowOpened;
            }
            AppWindow.Closing += OnWindowClosing;
        }

        private void EditorLoop()
        {
            EditorManager.Update();
            _engine.Update();
        }

        private void OnWindowOpened(object obj, RoutedEventArgs args)
        {
            EditorManager.LoadLayoutFromFile();
        }

        private void OnWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            args.Cancel = true;
            _engine.Dispose();
            if (EditorManager.IsLayoutSavedOnClose)
                EditorManager.SaveLayoutToFile();
            Close();
        }
    }
}
