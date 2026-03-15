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

    ImGui::Begin(_title.c_str());

    if (_drawFuntion)
        _drawFuntion();

    ImGui::End();

    if (_endFuntion)
        _endFuntion();
}

void EditorWindow::OnDestroy() {}

void EditorWindow::SetTitle(const winrt::hstring& title) 
{ 
	_title = winrt::to_string(title); 
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
