using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WsiuEngine.Core.System;
using WsiuRenderer;

namespace WsiuEngine.Core
{
    public class WsiuEngine
    {
        private EngineCore _engine;
        private InputSystem _inputSystem;

        public WsiuEngine(nint hwnd, SwapChainPanel enginePanel)
        {     
            _engine = new EngineCore();
            _engine.Initialize((ulong)hwnd, enginePanel);     
            _inputSystem = new InputSystem(_engine);
        }

        public void Update()
        {
            _engine.BeginFrame();
            _inputSystem.BeginFrame();
            _engine.Tick();
            _inputSystem.EndFrame();
            _engine.EndFrame();
        }
    }
}
