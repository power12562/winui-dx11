#include "pch.h"
#include "EditorWindowClosable.h"
#include "imguicommons.h"

void EditorWindowClosable::ImGuiBegin() 
{
    bool active = _active;
    ImGui::Begin(_title.c_str(), &active);
    if (active != _active)
    {
        SetActive(active);
    }
}
