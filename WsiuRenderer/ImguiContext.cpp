#include "pch.h"
#include "ImguiContext.h"
#if __has_include("ImguiContext.g.cpp")
#include "ImguiContext.g.cpp"
#endif
#include "imguicommons.h"
#include "imgui_helper.h"
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
        _engineCore.EditorWindowChangeTitle(_windowID, title); }

    void ImguiContext::PushID(uint32_t id) 
    { 
        auto pushID = [=]{ ImGui::PushID(id); };
        _beginCommands.emplace_back(pushID);
    }

    void ImguiContext::PopID()
    { 
       _endCommands.emplace_back(ImGui::PopID);
    }

    void ImguiContext::Text(hstring const& text)
    {
        auto textDraw = [=] { ImGui::Text(winrt::to_string(text).c_str()); };
        _beginCommands.emplace_back(textDraw);
    }

    void ImguiContext::SettingFloat(float speed, float min, float max, hstring const& format,
                                    winrt::WsiuRenderer::ImGuiSliderFlags const& flags)
    {
        _floatSetting.Speed = speed;
        _floatSetting.Min   = min;
        _floatSetting.Max   = max;
        _floatSetting.Format = winrt::to_string(format);
        _floatSetting.Flags  = flags;
    }

    void ImguiContext::DragFloat(hstring const& label, float val,
                                 winrt::WsiuRenderer::FloatChangedCallback const& handle)
    {
        auto& setting   = _floatSetting;
        auto  dragFloat = [=]() mutable
        {
            ImGuiHelper::DragScalaNWithCallback(winrt::to_string(label).c_str(), handle, 1, &val, setting.Speed,
                                                &setting.Min, &setting.Max, setting.Format.c_str(),
                                                static_cast<::ImGuiSliderFlags>(setting.Flags));
        };
        _beginCommands.emplace_back(dragFloat);
    }

    void ImguiContext::SettingDouble(float speed, double min, double max, hstring const& format,
                                     winrt::WsiuRenderer::ImGuiSliderFlags const& flags)
    {
        _doubleSetting.Speed  = speed;
        _doubleSetting.Min    = min;
        _doubleSetting.Max    = max;
        _doubleSetting.Format = winrt::to_string(format);
        _doubleSetting.Flags  = flags;
    }

    void ImguiContext::DragDouble(hstring const& label, double val,
                                  winrt::WsiuRenderer::DoubleChangedCallback const& handle)
    {
        auto& setting    = _doubleSetting;
        auto  dragDouble = [=]() mutable
        {
            ImGuiHelper::DragScalaNWithCallback(winrt::to_string(label).c_str(), handle, 1, &val, setting.Speed,
                                                &setting.Min, &setting.Max, setting.Format.c_str(),
                                                static_cast<::ImGuiSliderFlags>(setting.Flags));
        };
        _beginCommands.emplace_back(dragDouble);
    }


   
} // namespace winrt::WsiuRenderer::implementation
