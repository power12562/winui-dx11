#include "pch.h"
#include "ImguiContext.h"
#if __has_include("ImguiContext.g.cpp")
#include "ImguiContext.g.cpp"
#endif
#include "imguicommons.h"
#include "EngineCore.h"

namespace winrt::WsiuRenderer::implementation
{
    ImguiContext::ImguiContext(EngineCore const& engineCore) 
        : 
        _engineCore(engineCore)
    {}

    ImguiContext::~ImguiContext() 
    { 
        _engineCore.EditorWindowDestroy(_windowID); }

    void ImguiContext::InitializeWindow(hstring const& title) 
    {
        _windowID = _engineCore.EditorWindowCreate(title);
        _engineCore.EditorWindowDrawCallback(_windowID, [this] { DrawCommands(); });
    }

    void ImguiContext::DrawCommands() 
    {
        for (auto& func : _beginCommands)
        {
            func();
        }
        for (auto& func : _endCommands)
        {
            func();
        }
        _beginCommands.clear();
        _endCommands.clear();
    }

    void ImguiContext::SetTitle(hstring const& title) const 
    { 
        _engineCore.EditorWindowChangeTitle(_windowID, title);
    }

    void ImguiContext::Text(hstring const& text)
    {
        auto textDraw = [=] { ImGui::Text(winrt::to_string(text).c_str()); };
        _beginCommands.emplace_back(textDraw);
    }

} // namespace winrt::WsiuRenderer::implementation
