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

        private readonly EngineCore _engine;
        private readonly InputSystem _inputSystem;
        private readonly Time _time;

        public EngineCore EngineCore { get { return _engine; } }

        public Engine(nint hwnd, SwapChainPanel enginePanel)
        {     
            if (instance != null) throw new InvalidOperationException("Engine is already initialized!");

            instance = this;

            _engine = new EngineCore();
            _engine.Initialize((ulong)hwnd, enginePanel);

            _inputSystem = new InputSystem(_engine);
            InputSystem = _inputSystem;

            _time = new Time();
            Time = _time;
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
