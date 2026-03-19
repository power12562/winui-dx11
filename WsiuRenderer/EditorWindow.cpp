#include "pch.h"
#include "EditorWindow.h"
#include "imguicommons.h"

void EditorWindow::OnDraw() 
{ 
	if (_beginFunction)
        _beginFunction();

    if (_active)
    {
        ImGuiBegin();

        if (_drawFunction)
            _drawFunction();

        ImGui::End();
    }
  
    if (_endFunction)
        _endFunction();
}

void EditorWindow::ImGuiBegin() 
{
    ImGui::Begin(_title.c_str());
}