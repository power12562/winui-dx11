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
        if (0 < _stackDepth)
            throw winrt::hresult_error(E_FAIL, L"Command Stack Index is Invalid.");

        for (auto iter = _commands.begin(); iter != _commands.end(); ++iter)
        {
            auto& command = *iter;
            command();
            
            if (0 < _skipCommandCount)
            {
                iter += _skipCommandCount;
                _skipCommandCount = 0;
            }
        }
        ClearCommandsStack();
    }

    void ImguiContext::ClearCommandsStack() 
    { 
        _counterIndex = -1;
        _counterBegin = -1;
        _counterEnd   = 0;
        _stackDepth   = 0;
        _commandsStackCounter.clear();
        _commands.clear();
    }

    void ImguiContext::PushCommandsStack() 
    {
        ++_stackDepth;
        ++_counterIndex;
        if (_commandsStackCounter.size() == (size_t)_counterIndex)
        {
            _commandsStackCounter.emplace_back();
            _counterEnd = _counterIndex;
        }
    }

    void ImguiContext::PopCommandStack() 
    { 
        --_stackDepth;
        --_counterIndex;
        if (_counterIndex == _counterBegin)
        {
            _counterIndex = _counterEnd;
            _counterBegin = _counterEnd;
            ++_counterEnd;
        }
    }

    void ImguiContext::SetActive(bool active) 
    { 
        _engineCore.EditorWindowSetActive(_windowID, active);
    }

    void ImguiContext::SetTitle(hstring const& title) const 
    { 
        _engineCore.EditorWindowChangeTitle(_windowID, title); 
    }

    void ImguiContext::PushID(uint32_t id) 
    { 
        auto command = [id] { ImGui::PushID(id); };
        PushCommand(command);
    }

    void ImguiContext::PopID()
    { 
       PushCommand(ImGui::PopID); 
    }

    void ImguiContext::BeginDisabled() 
    {
        auto command = [] { ImGui::BeginDisabled(); };
        PushCommand(command);   
    }

    void ImguiContext::EndDisabled()  
    {
        auto command = [] { ImGui::EndDisabled(); };
        PushCommand(command);
    }

    void ImguiContext::TreeNodeEx(hstring const& label, winrt::WsiuRenderer::ImGuiTreeNodeFlags const& flags) 
    {
        auto command = [this, string = winrt::to_string(label), flags, counterID = StackCounterIndex()] 
        {
            if (ImGui::TreeNodeEx(string.c_str(), static_cast<ImGuiTreeNodeFlags_>(flags)) == false)
            {
                _skipCommandCount = _commandsStackCounter[counterID];
            }
        };
        PushCommand(command);
        PushCommandsStack();
    }

    void ImguiContext::TreePop() 
    {   
        auto command = [this] 
        { 
            ImGui::TreePop();         
        };  
        PushCommand(command);
        PopCommandStack();
    }

    void ImguiContext::PushStyleVar(winrt::WsiuRenderer::ImGuiStyleVar const& style, float x)
    {
        auto command = [style, x] { ImGui::PushStyleVar(static_cast<ImGuiStyleVar_>(style), {x}); };
        PushCommand(command);
    }

    void ImguiContext::PushStyleVar(winrt::WsiuRenderer::ImGuiStyleVar const& style, float x, float y) 
    {
        auto command = [style, x, y] { ImGui::PushStyleVar(static_cast<ImGuiStyleVar_>(style), {x, y}); };
        PushCommand(command);
    }   

    void ImguiContext::PopStyleVar() 
    { 
        auto command = [] { ImGui::PopStyleVar(); };
        PushCommand(command);
    }

    void ImguiContext::PopStyleVar(int count) 
    {
        auto command = [count] { ImGui::PopStyleVar(count); };
        PushCommand(command);
    }

    void ImguiContext::PushStyleColor(winrt::WsiuRenderer::ImGuiCol const& col, float x, float y, float z, float w) 
    {
    
    }

    void ImguiContext::PopStyleColor(int32_t count) 
    {
    
    }

    void ImguiContext::PopStyleColor() 
    {
    
    }

    void ImguiContext::Text(hstring const& text)
    {
        auto command = [text = winrt::to_string(text)] { ImGui::Text(text.c_str()); };
        PushCommand(command);
    }

    void ImguiContext::Button(hstring const& label, winrt::WsiuRenderer::ButtonCallback const& handle) 
    {
        Button(label, 0, 0, handle);
    }

    void ImguiContext::Button(hstring const& label, float x, float y, winrt::WsiuRenderer::ButtonCallback const& handle)
    {
        auto command = [label = winrt::to_string(label), x, y, handle]
        {          
            if (ImGui::Button(label.c_str(), {x, y}))
            {
                handle();
            }
        };
        PushCommand(command);
    }

    void ImguiContext::Selectable(hstring const& label, bool selected,
                                  winrt::WsiuRenderer::ImGuiSelectableFlags const& flags,
                                  winrt::WsiuRenderer::ButtonCallback const& handle)
    {
        Selectable(label, selected, flags, 0, 0, handle);
    }

    void ImguiContext::Selectable(hstring const& label, bool selected,
                                  winrt::WsiuRenderer::ImGuiSelectableFlags const& flags, float x, float y,
                                  winrt::WsiuRenderer::ButtonCallback const& handle)
    {
        auto command = [label = winrt::to_string(label), selected, flags, handle, x, y] () mutable
        {
            if(ImGui::Selectable(label.c_str(), selected, static_cast<ImGuiSelectableFlags_>(flags), {x, y}))
            {
                handle();
            }
        };
        PushCommand(command);
    }

    void ImguiContext::SettingFloat(float speed, float min, float max, hstring const& format,
                                    winrt::WsiuRenderer::ImGuiSliderFlags const& flags)
    {
        _floatSetting.Speed  = speed;
        _floatSetting.Min    = min;
        _floatSetting.Max    = max;
        _floatSetting.Format = winrt::to_string(format);
        _floatSetting.Flags  = flags;
    }

    void ImguiContext::DragFloat(hstring const& label, float val,
                                 winrt::WsiuRenderer::FloatChangedCallback const& handle)
    {
        auto command = [label = winrt::to_string(label), val, handle, setting = _floatSetting]() mutable
        {
            ImGuiHelper::DragScalaWithCallback(label.c_str(), handle, &val, setting.Speed,
                                                &setting.Min, &setting.Max, setting.Format.c_str(),
                                                static_cast<::ImGuiSliderFlags>(setting.Flags));
        };
        PushCommand(command);
    }

    void ImguiContext::DragFloatN(hstring const& label, array_view<float const> val,
                                  winrt::WsiuRenderer::FloatNChangedCallback const& handle)
    {
        if (4 < val.size())
            return;

        std::array<float, 4> temp{};
        std::copy(val.begin(), val.end(), temp.data());
        auto command = [label = winrt::to_string(label), temp, size = val.size(), handle, setting = _floatSetting]() mutable
        {
            ImGuiHelper::DragScalaNWithCallback(label.c_str(), handle, size, temp.data(),
                                                setting.Speed,
                                                &setting.Min, &setting.Max, setting.Format.c_str(),
                                                static_cast<::ImGuiSliderFlags>(setting.Flags));
        };
        PushCommand(command);
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
        auto command = [label = winrt::to_string(label), val, handle, setting = _doubleSetting]() mutable
        {
            ImGuiHelper::DragScalaWithCallback(label.c_str(), handle, &val, setting.Speed,
                                                &setting.Min, &setting.Max, setting.Format.c_str(),
                                                static_cast<::ImGuiSliderFlags>(setting.Flags));
        };
        PushCommand(command);
    }

    void ImguiContext::DragDoubleN(hstring const& label, array_view<double const> val,
                                   winrt::WsiuRenderer::DoubleNChangedCallback const& handle)
    {
        if (4 < val.size())
            return;

        std::array<double, 4> temp{};
        std::copy(val.begin(), val.end(), temp.data());
        auto command = [label = winrt::to_string(label), temp, size = val.size(), handle, setting = _doubleSetting]() mutable
        {
            ImGuiHelper::DragScalaNWithCallback(label.c_str(), handle, size, temp.data(),
                                                setting.Speed, &setting.Min, &setting.Max, setting.Format.c_str(),
                                                static_cast<::ImGuiSliderFlags>(setting.Flags));
        };
        PushCommand(command);
    }

} // namespace winrt::WsiuRenderer::implementation
