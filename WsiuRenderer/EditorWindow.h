#pragma once
#include "IWsiuGUI.h"

class EditorWindow : public IWsiuGUI
{
public:
    EditorWindow(const char* title) : _title(title) {};
    EditorWindow(const winrt::hstring& title);
    ~EditorWindow() override;
   
    void OnCreate() override;
    void OnDraw() override;
    void OnDestroy() override;

    void SetTitle(const winrt::hstring& title);

private:
    std::string _title;
};