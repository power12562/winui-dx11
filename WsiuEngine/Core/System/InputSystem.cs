using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using WsiuRenderer;

namespace WsiuEngine.Core.System
{
    public enum KeyState
    {
        None,       
        Down,       
        Held,       
        Up          
    }

    public class InputSystem(EngineCore engineCore)
    {
        private EngineCore _engine = engineCore;

        private KeyboardInputState _keyboardLastState;
        private KeyboardInputState _keyboardState;
        private MouseInputState _mouseInputLastState;
        private MouseInputState _mouseInputState;

        public KeyState GetKeyState(Keyboard.KeyCode keyCode)
        {
            bool last = IsKeyboardBitSet(ref _keyboardLastState, keyCode);
            bool current = IsKeyboardBitSet(ref _keyboardState, keyCode);
            if (current)
            {
                return last ? KeyState.Held : KeyState.Down;
            }
            return last ? KeyState.Up : KeyState.None;
        }

        public KeyState GetKeyState(Mouse.KeyCode keyCode)
        {
            bool last = IsMouseKeySet(ref _mouseInputLastState, keyCode);
            bool current = IsMouseKeySet(ref _mouseInputState, keyCode);
            if (current)
            {
                return last ? KeyState.Held : KeyState.Down;
            }
            return last ? KeyState.Up : KeyState.None;
        }

        private void UpdateMouse()
        {
            _mouseInputLastState = _mouseInputState;
            _mouseInputState = _engine.InputMouseState;
        }

        private static bool IsMouseKeySet(ref MouseInputState state, Mouse.KeyCode key)
        {
            switch (key)
            {
                case Mouse.KeyCode.Left:
                    return state.IsLeftDown;
                case Mouse.KeyCode.Right:
                    return state.IsRightDown;
                case Mouse.KeyCode.Middle:
                    return state.IsMiddleDown;
                case Mouse.KeyCode.X1:
                    return state.IsX1Down;
                case Mouse.KeyCode.X2:
                    return state.IsX2Down;
                default:
                    return false;
            }
        }

        private void UpdateKeyboard()
        {
            _keyboardLastState = _keyboardState;
            _keyboardState = _engine.InputKeyboardState;
        }

        private static unsafe bool IsKeyboardBitSet(ref KeyboardInputState state, Keyboard.KeyCode key)
        {
            int vk = (int)key;
            int index = vk / 32;
            int bitMask = 1 << (vk % 32);

            fixed (KeyboardInputState* ptr = &state)
            {
                uint* raw = (uint*)ptr;
                return (raw[index] & bitMask) != 0;
            }
        }

        internal void Update()
        {
            UpdateMouse();
            UpdateKeyboard();
        }
    }
}
