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
    public class WsiuEngine
    {
        private static WsiuEngine Instance { get; set; } = null!;
        public static InputSystem InputSystem { get; private set; } = null!;

        private readonly EngineCore _engine;
        private readonly InputSystem _inputSystem;

        public WsiuEngine(nint hwnd, SwapChainPanel enginePanel)
        {     
            if (Instance != null) throw new InvalidOperationException("Engine is already initialized!");

            Instance = this;

            _engine = new EngineCore();
            _engine.Initialize((ulong)hwnd, enginePanel);     

            _inputSystem = new InputSystem(_engine);
            InputSystem = _inputSystem;
        }

        public void Update()
        {
            _engine.BeginFrame();
            _inputSystem.Update();
            _engine.Tick();
            _engine.EndFrame();
        }
    }
}
