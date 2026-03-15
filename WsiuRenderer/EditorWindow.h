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
    void BeginCallback(const std::function<void()>& begin);
    void DrawCallback(const std::function<void()>& draw);
    void EndCallback(const std::function<void()>& end);

private:
    std::string _title;
    std::function<void()> _beginFuntion;
    std::function<void()> _drawFuntion;
    std::function<void()> _endFuntion;
};