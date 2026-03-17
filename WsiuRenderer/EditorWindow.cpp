#include "pch.h"
#include "EditorWindow.h"
#include "imguicommons.h"

EditorWindow::EditorWindow(const winrt::hstring& title) 
{ 
	_title = winrt::to_string(title);
}

EditorWindow::~EditorWindow() = default;

void EditorWindow::OnCreate() 
{

}

void EditorWindow::OnDraw() 
{ 
	if (_beginFuntion)
        _beginFuntion();

    if (_active)
    {
        ImGuiBegin();

        if (_drawFuntion)
            _drawFuntion();

        ImGui::End();
    }
  
    if (_endFuntion)
        _endFuntion();
}

void EditorWindow::OnDestroy() 
{

}

void EditorWindow::ImGuiBegin() 
{
    ImGui::Begin(_title.c_str());
}

void EditorWindow::SetTitle(const winrt::hstring& title) 
{ 
	_title = winrt::to_string(title); 
}

void EditorWindow::SetActive(bool active) 
{ 
    _active = active;
}

bool EditorWindow::GetActive() const 
{
    return _active; 
}

void EditorWindow::BeginCallback(const std::function<void()>& begin) 
{ 
	_beginFuntion = begin; 
}

void EditorWindow::DrawCallback(const std::function<void()>& draw) 
{ 
	_drawFuntion = draw; 
}

void EditorWindow::EndCallback(const std::function<void()>& end) 
{
	_endFuntion = end;
}
