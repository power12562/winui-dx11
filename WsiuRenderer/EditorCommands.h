#pragma once
#include "IWsiuGui.h"

class EditorCommands : public IWsiuGUI
{
public:
    EditorCommands(const char* title) : _title(title) {};
    EditorCommands(const winrt::hstring& title);
    ~EditorCommands() override; 

    void OnCreate() override {};
    void OnDraw() override;
    void OnDestroy() override {}

    void SetTitle(const winrt::hstring& title);
    void SetActive(bool active);
    bool GetActive() const;
    void BeginCallback(const std::function<void()>& begin);
    void DrawCallback(const std::function<void()>& draw);
    void EndCallback(const std::function<void()>& end);
    void DisableCallback(const std::function<void()>& disable);

protected:
    std::string _title;
    bool _active = true;
    std::function<void()> _beginFunction;
    std::function<void()> _drawFunction;
    std::function<void()> _endFunction;
    std::function<void()> _disableFunction;
};
