#include "pch.h"
#include "imgui_helper.h"

namespace ImGuiHelper
{
    ImGuiKey VirtualKeyToImGuiKey(WPARAM wParam)
    {
        switch (wParam)
        {
        // 숫자 키
        case '0':
            return ImGuiKey_0;
        case '1':
            return ImGuiKey_1;
        case '2':
            return ImGuiKey_2;
        case '3':
            return ImGuiKey_3;
        case '4':
            return ImGuiKey_4;
        case '5':
            return ImGuiKey_5;
        case '6':
            return ImGuiKey_6;
        case '7':
            return ImGuiKey_7;
        case '8':
            return ImGuiKey_8;
        case '9':
            return ImGuiKey_9;

        // 알파벳 키
        case 'A':
            return ImGuiKey_A;
        case 'B':
            return ImGuiKey_B;
        case 'C':
            return ImGuiKey_C;
        case 'D':
            return ImGuiKey_D;
        case 'E':
            return ImGuiKey_E;
        case 'F':
            return ImGuiKey_F;
        case 'G':
            return ImGuiKey_G;
        case 'H':
            return ImGuiKey_H;
        case 'I':
            return ImGuiKey_I;
        case 'J':
            return ImGuiKey_J;
        case 'K':
            return ImGuiKey_K;
        case 'L':
            return ImGuiKey_L;
        case 'M':
            return ImGuiKey_M;
        case 'N':
            return ImGuiKey_N;
        case 'O':
            return ImGuiKey_O;
        case 'P':
            return ImGuiKey_P;
        case 'Q':
            return ImGuiKey_Q;
        case 'R':
            return ImGuiKey_R;
        case 'S':
            return ImGuiKey_S;
        case 'T':
            return ImGuiKey_T;
        case 'U':
            return ImGuiKey_U;
        case 'V':
            return ImGuiKey_V;
        case 'W':
            return ImGuiKey_W;
        case 'X':
            return ImGuiKey_X;
        case 'Y':
            return ImGuiKey_Y;
        case 'Z':
            return ImGuiKey_Z;

        // 특수 키 및 화살표
        case VK_TAB:
            return ImGuiKey_Tab;
        case VK_LEFT:
            return ImGuiKey_LeftArrow;
        case VK_RIGHT:
            return ImGuiKey_RightArrow;
        case VK_UP:
            return ImGuiKey_UpArrow;
        case VK_DOWN:
            return ImGuiKey_DownArrow;
        case VK_PRIOR:
            return ImGuiKey_PageUp;
        case VK_NEXT:
            return ImGuiKey_PageDown;
        case VK_HOME:
            return ImGuiKey_Home;
        case VK_END:
            return ImGuiKey_End;
        case VK_INSERT:
            return ImGuiKey_Insert;
        case VK_DELETE:
            return ImGuiKey_Delete;
        case VK_BACK:
            return ImGuiKey_Backspace;
        case VK_SPACE:
            return ImGuiKey_Space;
        case VK_RETURN:
            return ImGuiKey_Enter;
        case VK_ESCAPE:
            return ImGuiKey_Escape;

        // 기능 키 (F1~F12)
        case VK_F1:
            return ImGuiKey_F1;
        case VK_F2:
            return ImGuiKey_F2;
        case VK_F3:
            return ImGuiKey_F3;
        case VK_F4:
            return ImGuiKey_F4;
        case VK_F5:
            return ImGuiKey_F5;
        case VK_F6:
            return ImGuiKey_F6;
        case VK_F7:
            return ImGuiKey_F7;
        case VK_F8:
            return ImGuiKey_F8;
        case VK_F9:
            return ImGuiKey_F9;
        case VK_F10:
            return ImGuiKey_F10;
        case VK_F11:
            return ImGuiKey_F11;
        case VK_F12:
            return ImGuiKey_F12;

        // 모디파이어 키
        case VK_LSHIFT:
        case VK_SHIFT:
            return ImGuiMod_Shift;
        case VK_RSHIFT:
            return ImGuiKey_RightShift;
        case VK_LCONTROL:
        case VK_CONTROL:
            return ImGuiMod_Ctrl;
        case VK_RCONTROL:
            return ImGuiKey_RightCtrl;
        case VK_LMENU:
        case VK_MENU:
            return ImGuiMod_Alt;
        case VK_RMENU:
            return ImGuiKey_RightAlt;
        case VK_LWIN:
            return ImGuiMod_Shift;
        case VK_RWIN:
            return ImGuiKey_RightSuper;
        default:
            return ImGuiKey_None;
        }
    }
} // namespace ImGuiHelper