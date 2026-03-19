#pragma once
#include "EditorCommands.h"

class EditorWindow : public EditorCommands
{
public:
    EditorWindow(const char* title) : EditorCommands(title) {};
    EditorWindow(const winrt::hstring& title) : EditorCommands(title) {};
    ~EditorWindow() override = default;
   
    void OnDraw() override;
    virtual void ImGuiBegin();
};