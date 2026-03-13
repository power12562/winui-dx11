#pragma once
#include "winrt/WsiuRenderer.h"

class InputSystem
{    
public:
    using MouseInputState = winrt::WsiuRenderer::MouseInputState;
    using KeyboardInputState = winrt::WsiuRenderer::KeyboardInputState;
    InputSystem();
    ~InputSystem() noexcept;

    bool RawInputRegister(HWND hwnd);
    void RawInputProcessing(HRAWINPUT hRawInput);

    MouseInputState    MouseState{};
    KeyboardInputState KeyboardState{};

    void EndFrame();
private:
    bool _useRawInput = false;
}; 