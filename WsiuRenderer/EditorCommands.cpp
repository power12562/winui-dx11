#include "pch.h"
#include "EditorCommands.h"
EditorCommands::EditorCommands(const winrt::hstring& title)
{
    _title = winrt::to_string(title);
}

EditorCommands::~EditorCommands() = default;

void EditorCommands::OnDraw()
{
    if (_beginFunction)
        _beginFunction();

    if (_active)
    {
        if (_drawFunction)
            _drawFunction();
    }

    if (_endFunction)
        _endFunction();
}

void EditorCommands::SetTitle(const winrt::hstring& title)
{
    _title = winrt::to_string(title);
}

void EditorCommands::SetActive(bool active)
{
    _active = active;
    if (_active == false && _disableFunction)
    {
        _disableFunction();
    }
}

bool EditorCommands::GetActive() const
{
    return _active;
}

void EditorCommands::BeginCallback(const std::function<void()>& begin)
{
    _beginFunction = begin;
}

void EditorCommands::DrawCallback(const std::function<void()>& draw)
{
    _drawFunction = draw;
}

void EditorCommands::EndCallback(const std::function<void()>& end)
{
    _endFunction = end;
}

void EditorCommands::DisableCallback(const std::function<void()>& disable)
{
    _disableFunction = disable;
}
