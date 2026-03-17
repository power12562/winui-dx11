#pragma once
#include "EditorWindow.h"

class EditorWindowClosable : public EditorWindow
{
public:
    EditorWindowClosable(const char* title) : EditorWindow(title) {};
    EditorWindowClosable(const winrt::hstring& title) : EditorWindow(title) {}

    void ImGuiBegin() override;
};