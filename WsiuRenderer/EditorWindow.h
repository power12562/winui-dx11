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

    virtual void ImGuiBegin();

    void SetTitle(const winrt::hstring& title);
    void SetActive(bool active);
    bool GetActive() const;
    void BeginCallback(const std::function<void()>& begin);
    void DrawCallback(const std::function<void()>& draw);
    void EndCallback(const std::function<void()>& end);

protected:
    std::string _title;
    bool _active = true;
    
private:
    std::function<void()> _beginFuntion;
    std::function<void()> _drawFuntion;
    std::function<void()> _endFuntion;
};