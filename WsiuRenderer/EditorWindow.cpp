#include "pch.h"
#include "EditorWindow.h"
#include "imguicommons.h"

EditorWindow::EditorWindow(const winrt::hstring& title) 
{ 
	_title = winrt::to_string(title);
}

EditorWindow::~EditorWindow() = default;

void EditorWindow::OnCreate() {}

void EditorWindow::OnDraw() 
{ 
	ImGui::Begin(_title.c_str());
    ImGui::End();
}

void EditorWindow::OnDestroy() {}

void EditorWindow::SetTitle(const winrt::hstring& title) 
{ 
	_title = winrt::to_string(title); 
}
