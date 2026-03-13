using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WsiuRenderer;

namespace WsiuEngine.Core.System
{
    public class InputSystem(EngineCore engineCore)
    {
        private EngineCore _engine = engineCore;

        private KeyboardInputState _keyboardState;
        private MouseInputState _mouseInputState;
        private KeyboardInputState _keyboardLastState;
        private MouseInputState _mouseInputLastState;

        internal void BeginFrame()
        {
            _mouseInputState = _engine.InputMouseState;
            _keyboardState = _engine.InputKeyboardState;
        }

        internal void EndFrame()
        {
            _mouseInputLastState = _mouseInputState;
            _keyboardLastState = _keyboardState;
        }
    }
}
