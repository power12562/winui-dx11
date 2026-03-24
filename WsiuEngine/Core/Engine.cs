using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using WsiuEngine.Core.System;
using WsiuRenderer;

namespace WsiuEngine.Core
{
    public class Engine
    {
        private static Engine instance { get; set; } = null!;
        public static InputSystem InputSystem { get; private set; } = null!;
        public static Time Time { get; private set; } = null!;
        public static Screen Screen { get; private set; } = null!;

        private readonly EngineCore _engine;
        private readonly InputSystem _inputSystem;
        private readonly Time _time;
        private readonly Screen _screen;

        public EngineCore EngineCore { get { return _engine; } }

        public Engine(nint hwnd, SwapChainPanel enginePanel)
        {     
            if (instance != null) 
                throw new InvalidOperationException("Engine is already initialized!");
            instance = this;

            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            _engine = new EngineCore();
            _engine.Initialize((ulong)hwnd, enginePanel);

            _inputSystem = new InputSystem(_engine);
            InputSystem = _inputSystem;

            _time = new Time();
            Time = _time;

            _screen = new Screen(appWindow);
            Screen = _screen;
        }

        public void Update()
        {
            _time.UpdateTime();
            _engine.BeginFrame();
            _inputSystem.Update();
            _engine.Tick();
            _engine.EndFrame();
        }
    }
}
