using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WsiuEngine.Core;
using WsiuEngine.Core.System;
using WsiuRenderer;

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
        private readonly ImguiContext _timeDebugUI;

        public MainWindow()
        {
            InitializeComponent();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            _engine = new Engine(hwnd, EnginePanel);

            _timeDebugUI = new(_engine.EngineCore);
            _timeDebugUI.InitializeWindow("Time");

            CompositionTarget.Rendering += (sender, args) => EditorLoop();
        }

        private void EditorLoop()
        {
            TimeDraw();
            _engine.Update();
        }

        private void TimeDraw()
        {
            _timeDebugUI.SettingDouble(0.01f, 0, 0, "%.3f", ImGuiSliderFlags.None);
            foreach (var field in ReflectedType<Time>.Fields)
            {
                if(field.Type == typeof(double))
                {
                    if (field.Set == null)
                    {                       
                        if (field.Get(Engine.Time) is double f)
                        {
                            _timeDebugUI.BeginDisabled();
                            _timeDebugUI.DragDouble(field.Name, f, (val) => {});
                            _timeDebugUI.EndDisabled();
                        }
                    }
                    else
                    {
                        if (field.Get(Engine.Time) is double f)
                        {                      
                            _timeDebugUI.DragDouble(field.Name, f, (val) =>
                            {
                                field.Set(Engine.Time, val);
                            });
                        }
                    }
                }            
            }                    
        }
    }
}
