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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using WsiuRenderer;

namespace WsiuEngine
{
    namespace Core
    {
        namespace MainEntry
        {
            /// <summary>
            /// An empty window that can be used on its own or navigated to within a Frame.
            /// </summary>
            public sealed partial class MainWindow : Window
            {
                public MainWindow()
                {
                    InitializeComponent();
                    InitializeEngineCore();
                }

                private EngineCore? _engine;

                private void InitializeEngineCore()
                {
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                    _engine = new EngineCore();
                    _engine.Initialize((ulong)hwnd, EnginePanel);
                    CompositionTarget.Rendering += (sender, args) => Update();
                }

                private void Update()
                {
                    if (_engine == null) return;

                    _engine.BeginFrame();
                    _engine.Tick();
                    _engine.EndFrame();
                }
            }
        }
    }
}
