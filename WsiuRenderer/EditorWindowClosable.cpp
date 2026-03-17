#include "pch.h"
#include "EditorWindowClosable.h"
#include "imguicommons.h"

void EditorWindowClosable::ImGuiBegin() 
{
	ImGui::Begin(_title.c_str(), &_active);
}
