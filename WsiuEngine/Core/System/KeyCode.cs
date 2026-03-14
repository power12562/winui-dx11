namespace WsiuEngine.Core.System
{
    namespace Mouse
    {
        public enum KeyCode : int
        {
            None = 0,
            Left = 0x01,
            Right = 0x02,
            Middle = 0x03,
            X1 = 0x04,
            X2 = 0x05
        }
    }

    namespace Keyboard
    {
        public enum KeyCode : int
        {
            None = 0,

            // 제어키
            Backspace = 0x08,
            Tab = 0x09,
            Clear = 0x0C,
            Return = 0x0D,
            Pause = 0x13,
            Escape = 0x1B,
            Space = 0x20,

            // 특수키
            PageUp = 0x21,
            PageDown = 0x22,
            End = 0x23,
            Home = 0x24,
            LeftArrow = 0x25,
            UpArrow = 0x26,
            RightArrow = 0x27,
            DownArrow = 0x28,
            Insert = 0x2D,
            Delete = 0x2E,

            // 숫자키 
            Alpha0 = 0x30, Alpha1 = 0x31, Alpha2 = 0x32, Alpha3 = 0x33, Alpha4 = 0x34,
            Alpha5 = 0x35, Alpha6 = 0x36, Alpha7 = 0x37, Alpha8 = 0x38, Alpha9 = 0x39,

            // 알파벳 
            A = 0x41, B = 0x42, C = 0x43, D = 0x44, E = 0x45, F = 0x46, G = 0x47,
            H = 0x48, I = 0x49, J = 0x4A, K = 0x4B, L = 0x4C, M = 0x4D, N = 0x4E,
            O = 0x4F, P = 0x50, Q = 0x51, R = 0x52, S = 0x53, T = 0x54, U = 0x55,
            V = 0x56, W = 0x57, X = 0x58, Y = 0x59, Z = 0x5A,

            // 윈도우 키
            LeftWindows = 0x5B,
            RightWindows = 0x5C,

            // 키패드
            Keypad0 = 0x60, Keypad1 = 0x61, Keypad2 = 0x62, Keypad3 = 0x63,
            Keypad4 = 0x64, Keypad5 = 0x65, Keypad6 = 0x66, Keypad7 = 0x67,
            Keypad8 = 0x68, Keypad9 = 0x69,
            KeypadMultiply = 0x6A, KeypadPlus = 0x6B, KeypadMinus = 0x6D,
            KeypadPeriod = 0x6E, KeypadDivide = 0x6F,

            // F키
            F1 = 0x70, F2 = 0x71, F3 = 0x72, F4 = 0x73, F5 = 0x74, F6 = 0x75,
            F7 = 0x76, F8 = 0x77, F9 = 0x78, F10 = 0x79, F11 = 0x7A, F12 = 0x7B,

            // 상태키
            Numlock = 0x90,
            ScrollLock = 0x91,

            // 조합키 
            LeftShift = 0xA0,
            RightShift = 0xA1,
            LeftControl = 0xA2,
            RightControl = 0xA3,
            LeftAlt = 0xA4,
            RightAlt = 0xA5,

            // 기호 및 기타
            Semicolon = 0xBA,
            Plus = 0xBB,
            Comma = 0xBC,
            Minus = 0xBD,
            Period = 0xBE,
            Slash = 0xBF,
            BackQuote = 0xC0,
            LeftBracket = 0xDB,
            Backslash = 0xDC,
            RightBracket = 0xDD,
            Quote = 0xDE
        }
    }
}