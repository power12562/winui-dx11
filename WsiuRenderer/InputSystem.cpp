#include "pch.h"
#include "InputSystem.h"

InputSystem::InputSystem() = default;
InputSystem::~InputSystem() noexcept = default;

bool InputSystem::RawInputRegister(HWND hwnd) 
{
    RAWINPUTDEVICE rid[2] = {0};
    // 마우스 등록
    rid[0].usUsagePage = 0x01;          
    rid[0].usUsage     = 0x02;            
    rid[0].hwndTarget  = hwnd;

    // 키보드 등록
    rid[1].usUsagePage = 0x01;
    rid[1].usUsage     = 0x06; 
    rid[1].hwndTarget  = hwnd;

    if (RegisterRawInputDevices(rid, 2, sizeof(RAWINPUTDEVICE)) == FALSE)
	    return false; 

    _useRawInput = true;
    return true;
}

void InputSystem::RawInputProcessing(HRAWINPUT hRawInput) 
{
    if (_useRawInput == false)
        return;

    UINT dwSize;
    GetRawInputData(hRawInput, RID_INPUT, NULL, &dwSize, sizeof(RAWINPUTHEADER));

    alignas(RAWINPUT) BYTE   fixedBuffer[64]{};
    static std::vector<BYTE> dynamicBuffer;

    BYTE* bufferPtr = nullptr;
    if (dwSize <= 64)
    {
        bufferPtr = fixedBuffer;
    }
    else
    {
        if (dynamicBuffer.size() < dwSize)
        {
            dynamicBuffer.resize(dwSize);
        }
        bufferPtr = dynamicBuffer.data();
    }

    if (GetRawInputData(hRawInput, RID_INPUT, bufferPtr, &dwSize, sizeof(RAWINPUTHEADER)) != (UINT)-1)
    {
        RAWINPUT* raw = reinterpret_cast<RAWINPUT*>(bufferPtr);      
        if (raw->header.dwType == RIM_TYPEMOUSE)
        {
            const RAWMOUSE& mouse = raw->data.mouse;
            if (!(mouse.usFlags & MOUSE_MOVE_ABSOLUTE))
            {
                MouseState.DeltaX += mouse.lLastX;
                MouseState.DeltaY += mouse.lLastY;
            }

            if (mouse.usButtonFlags & RI_MOUSE_LEFT_BUTTON_DOWN)
                MouseState.IsLeftDown = true;
            if (mouse.usButtonFlags & RI_MOUSE_LEFT_BUTTON_UP)
                MouseState.IsLeftDown = false;

            if (mouse.usButtonFlags & RI_MOUSE_RIGHT_BUTTON_DOWN)
                MouseState.IsRightDown = true;
            if (mouse.usButtonFlags & RI_MOUSE_RIGHT_BUTTON_UP)
                MouseState.IsRightDown = false;

            if (mouse.usButtonFlags & RI_MOUSE_BUTTON_4_DOWN)
                MouseState.IsX1Down = true;
            if (mouse.usButtonFlags & RI_MOUSE_BUTTON_4_UP)
                MouseState.IsX1Down = false;

            if (mouse.usButtonFlags & RI_MOUSE_BUTTON_5_DOWN)
                MouseState.IsX2Down = true;
            if (mouse.usButtonFlags & RI_MOUSE_BUTTON_5_UP)
                MouseState.IsX2Down = false;

            if (mouse.usButtonFlags & RI_MOUSE_WHEEL)
            {
                MouseState.WheelDelta += static_cast<short>(mouse.usButtonData);
            }
        }
        else if (raw->header.dwType == RIM_TYPEKEYBOARD)
        {
            const RAWKEYBOARD& keyboard = raw->data.keyboard;

            USHORT vkey   = keyboard.VKey;
            bool   isDown = !(keyboard.Flags & RI_KEY_BREAK);

            int index = vkey / 32;
            int bit   = vkey % 32;

            uint32_t* rawBits = reinterpret_cast<uint32_t*>(&KeyboardState);
            if (isDown)
                rawBits[index] |= (1 << bit);
            else
                rawBits[index] &= ~(1 << bit);
        }
    }
}

void InputSystem::EndFrame() 
{ 
    if (_useRawInput)
    {
        MouseState.DeltaX     = 0;
        MouseState.DeltaY     = 0;
        MouseState.WheelDelta = 0;
    }
}
