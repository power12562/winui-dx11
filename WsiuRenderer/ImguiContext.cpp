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

    void ImguiContext::InitializeWindowClosable(hstring const& title) 
    {
        _windowID = _engineCore.EditorWindowClosableCreate(title);
        _engineCore.EditorWindowDrawCallback(_windowID, [this] { DrawCommands(); });
    }

    void ImguiContext::DrawCommands() 
    {
        for (auto& func : _commands)
        {
            func();
        }
        _commands.clear();
    }

    void ImguiContext::SetActive(bool active) 
    { 
        _engineCore.EditorWindowSetActive(_windowID, active);
    }

    void ImguiContext::SetTitle(hstring const& title) const 
    { 
        _engineCore.EditorWindowChangeTitle(_windowID, title); }

    void ImguiContext::PushID(uint32_t id) 
    { 
        auto pushID = [=]{ ImGui::PushID(id); };
        _commands.emplace_back(pushID);
    }

    void ImguiContext::PopID()
    { 
       _commands.emplace_back(ImGui::PopID); 
    }

    void ImguiContext::BeginDisabled() 
    {
        auto disable = [] { ImGui::BeginDisabled(); };
        _commands.emplace_back(disable);
    }

    void ImguiContext::EndDisabled()  
    {
        auto disable = [] { ImGui::EndDisabled(); };
        _commands.emplace_back(disable);
    }

    void ImguiContext::PushStyleVar(winrt::WsiuRenderer::ImGuiStyleVar const& style, float x)
    {
        auto command = [=] { ImGui::PushStyleVar(static_cast<ImGuiStyleVar_>(style), {x}); };
        _commands.emplace_back(command);
    }

    void ImguiContext::PushStyleVar(winrt::WsiuRenderer::ImGuiStyleVar const& style, float x, float y) 
    {
        auto command = [=] { ImGui::PushStyleVar(static_cast<ImGuiStyleVar_>(style), {x, y}); };
        _commands.emplace_back(command);
    }   

    void ImguiContext::PopStyleVar() 
    { 
        auto command = [] { ImGui::PopStyleVar(); };
        _commands.emplace_back(command);
    }

    void ImguiContext::PopStyleVar(int count) 
    {
        auto command = [count] { ImGui::PopStyleVar(count); };
        _commands.emplace_back(command);
    }

    void ImguiContext::Text(hstring const& text)
    {
        auto textDraw = [text] { ImGui::Text(winrt::to_string(text).c_str()); };
        _commands.emplace_back(textDraw);
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
        auto dragFloat = [label, val, handle, setting = _floatSetting]() mutable
        {
            ImGuiHelper::DragScalaWithCallback(winrt::to_string(label).c_str(), handle, &val, setting.Speed,
                                                &setting.Min, &setting.Max, setting.Format.c_str(),
                                                static_cast<::ImGuiSliderFlags>(setting.Flags));
        };
        _commands.emplace_back(dragFloat);
    }

    void ImguiContext::DragFloatN(hstring const& label, array_view<float const> val,
                                  winrt::WsiuRenderer::FloatNChangedCallback const& handle)
    {
        if (4 < val.size())
            return;

        std::array<float, 4> temp{};
        std::copy(val.begin(), val.end(), temp.data());
        auto dragFloat = [label, temp, size = val.size(), handle, setting = _floatSetting]() mutable
        {
            ImGuiHelper::DragScalaNWithCallback(winrt::to_string(label).c_str(), handle, size, temp.data(),
                                                setting.Speed,
                                                &setting.Min, &setting.Max, setting.Format.c_str(),
                                                static_cast<::ImGuiSliderFlags>(setting.Flags));
        };
        _commands.emplace_back(dragFloat);
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
        auto  dragDouble = [label, val, handle, setting = _doubleSetting]() mutable
        {
            ImGuiHelper::DragScalaWithCallback(winrt::to_string(label).c_str(), handle, &val, setting.Speed,
                                                &setting.Min, &setting.Max, setting.Format.c_str(),
                                                static_cast<::ImGuiSliderFlags>(setting.Flags));
        };
        _commands.emplace_back(dragDouble);
    }

    void ImguiContext::DragDoubleN(hstring const& label, array_view<double const> val,
                                   winrt::WsiuRenderer::DoubleNChangedCallback const& handle)
    {
        if (4 < val.size())
            return;

        std::array<double, 4> temp{};
        std::copy(val.begin(), val.end(), temp.data());
        auto dragDouble = [label, temp, size = val.size(), handle, setting = _doubleSetting]() mutable
        {
            ImGuiHelper::DragScalaNWithCallback(winrt::to_string(label).c_str(), handle, size, temp.data(),
                                                setting.Speed, &setting.Min, &setting.Max, setting.Format.c_str(),
                                                static_cast<::ImGuiSliderFlags>(setting.Flags));
        };
        _commands.emplace_back(dragDouble);
    }

} // namespace winrt::WsiuRenderer::implementation
