using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WsiuRenderer;
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
        private Engine _engine;
        private ImguiContext _inspector;
        private UInt64 _frameCount;
        private double _lastTime = 0;
        private UInt64 _fps = 0;
        private System.Diagnostics.Stopwatch _stopwatch = new();

        public MainWindow()
        {
            InitializeComponent();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            _engine = new Engine(hwnd, EnginePanel);

            _inspector = new(_engine.EngineCore);
            _inspector.InitializeWindow("Inspector");

            CompositionTarget.Rendering += (sender, args) => EditorLoop();

            _stopwatch.Start();
        }

        private void EditorLoop()
        {
            InspectorDraw();
            _engine.Update();
        }

        private void InspectorDraw()
        {
            ++_frameCount;
            double currentTime = _stopwatch.Elapsed.TotalSeconds;
            if (currentTime - _lastTime >= 1.0)
            {
                _fps = _frameCount;
                _frameCount = 0;
                _lastTime = currentTime;
            }
            _inspector.Text($"Current Frame: {_frameCount}");
            _inspector.Text($"FPS: {_fps}");
            _inspector.Text($"Engine Time: {DateTime.Now:HH:mm:ss}");
        }
    }
}
