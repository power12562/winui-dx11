using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace WsiuEngine.Core.MainEntry
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private Engine _engine;

        public MainWindow()
        {
            InitializeComponent();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            _engine = new Engine(this, EnginePanel);
            CompositionTarget.Rendering += (sender, args) => _engine.Update();
        }
    }
}
