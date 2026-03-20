#include "pch.h"
#include "ImguiContext.h"
#if __has_include("ImguiContext.g.cpp")
#include "ImguiContext.g.cpp"
#endif
#include "EngineCore.h"
#include "imgui_helper.h"
#include "imguicommons.h"

namespace winrt::WsiuRenderer::implementation
{
    ImguiContext::ImguiContext(EngineCore const& engineCore) : _engineCore(engineCore)
    {
    }

    ImguiContext::~ImguiContext()
    {
        _engineCore.EditorDestroy(_windowID);
    }

    void ImguiContext::InitializeCommands(hstring const& title)
    {
        _windowID = _engineCore.EditorCommandsCreate(title);
        _engineCore.EditorDrawCallback(_windowID,
                                       [this]
                                       {
                                           DrawCommands();
                                       });
    }

    void ImguiContext::InitializeWindow(hstring const& title)
    {
        _windowID = _engineCore.EditorWindowCreate(title);
        _engineCore.EditorDrawCallback(_windowID,
                                       [this]
                                       {
                                           DrawCommands();
                                       });
    }

    void ImguiContext::InitializeWindowClosable(hstring const& title)
    {
        _windowID = _engineCore.EditorWindowClosableCreate(title);
        _engineCore.EditorDrawCallback(_windowID,
                                       [this]
                                       {
                                           DrawCommands();
                                       });
    }

    void ImguiContext::DrawCommands()
    {
        if (0 < _stackDepth)
            throw winrt::hresult_error(E_FAIL, L"Command Stack Index is Invalid.");

        for (size_t i = 0; i < _commands.size(); ++i)
        {
            auto& command = _commands[i];
            command();

            if (0 < _skipCommandCount)
            {
                i += _skipCommandCount;
                _skipCommandCount = 0;
            }
        }
        ClearCommandsStack();
    }

    void ImguiContext::ClearCommandsStack()
    {
        _stackDepth = 0;
        _commandsStackCounter.clear();
        _commands.clear();
    }

    void ImguiContext::SkipCommand(size_t counterId)
    {
        size_t count      = _commandsStackCounter.at(counterId);
        _skipCommandCount = count;
    }

    void ImguiContext::PushCommandStack(size_t counterId)
    {
        ++_stackDepth;
        _commandsStack.push_back(counterId);
    }

    void ImguiContext::PopCommandStack()
    {
        --_stackDepth;
        _commandsStack.pop_back();
    }

    bool ImguiContext::GetActive() const
    {
        return _engineCore.EditorGetActive(_windowID);
    }

    void ImguiContext::SetActive(bool active)
    {
        _engineCore.EditorSetActive(_windowID, active);
    }

    void ImguiContext::SetTitle(hstring const& title) const
    {
        _engineCore.EditorChangeTitle(_windowID, title);
    }

    void ImguiContext::Separator() 
    {
        auto command = []
        {
            ImGui::Separator();
        };
        PushCommand(command);
    }

    void ImguiContext::SeparatorText(hstring const& text)
    {
        auto command = [text = winrt::to_string(text)]
        {
            ImGui::SeparatorText(text.c_str());
        };
        PushCommand(command);
    }

    void ImguiContext::PushID(uint32_t id)
    {
        auto command = [id]
        {
            ImGui::PushID(id);
        };
        PushCommand(command);
    }

    void ImguiContext::PopID()
    {
        PushCommand(ImGui::PopID);
    }

    void ImguiContext::BeginDisabled()
    {
        auto command = []
        {
            ImGui::BeginDisabled();
        };
        PushCommand(command);
    }

    void ImguiContext::EndDisabled()
    {
        auto command = []
        {
            ImGui::EndDisabled();
        };
        PushCommand(command);
    }

    void ImguiContext::BeginMainMenuBar()
    {
        size_t counterID = _commandsStackCounter.create();
        auto   command   = [this, counterID]
        {
            if (ImGui::BeginMainMenuBar() == false)
            {
                SkipCommand(counterID);
            }
        };
        PushCommand(command);
        PushCommandStack(counterID);
    }

    void ImguiContext::EndMainMenuBar()
    {
        auto command = []
        {
            ImGui::EndMainMenuBar();
        };
        PushCommand(command);
        PopCommandStack();
    }

    void ImguiContext::BeginMenu(hstring const& label)
    {
        BeginMenu(label, true);
    }

    void ImguiContext::BeginMenu(hstring const& label, bool enabled)
    {
        size_t counterID = _commandsStackCounter.create();
        auto   command   = [this, label = winrt::to_string(label), enabled, counterID]
        {
            if (ImGui::BeginMenu(label.c_str(), enabled) == false)
            {
                SkipCommand(counterID);
            }
        };
        PushCommand(command);
        PushCommandStack(counterID);
    }

    void ImguiContext::EndMenu()
    {
        auto command = []
        {
            ImGui::EndMenu();
        };
        PushCommand(command);
        PopCommandStack();
    }

    void ImguiContext::MenuItem(hstring const& label, winrt::WsiuRenderer::ButtonCallback const& handle)
    {
        MenuItem(label, false, true, handle);
    }

    void ImguiContext::MenuItem(hstring const& label, bool selected, winrt::WsiuRenderer::ButtonCallback const& handle)
    {
        MenuItem(label, selected, true, handle);
    }

    void ImguiContext::MenuItem(hstring const&                             label,
                                bool                                       selected,
                                bool                                       enabled,
                                winrt::WsiuRenderer::ButtonCallback const& handle)
    {
        auto command = [label = winrt::to_string(label), selected, enabled, handle]
        {
            if (ImGui::MenuItem(label.c_str(), nullptr, selected, enabled))
            {
                handle();
            }
        };
        PushCommand(command);
    }

    void ImguiContext::TreeNodeEx(hstring const& label, winrt::WsiuRenderer::ImGuiTreeNodeFlags const& flags)
    {
        size_t counterID = _commandsStackCounter.create();
        auto   command   = [this, string = winrt::to_string(label), flags, counterID]
        {
            if (ImGui::TreeNodeEx(string.c_str(), static_cast<ImGuiTreeNodeFlags_>(flags)) == false)
            {
                SkipCommand(counterID);
            }
        };
        PushCommand(command);
        PushCommandStack(counterID);
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
        auto command = [style, x]
        {
            ImGui::PushStyleVar(static_cast<ImGuiStyleVar_>(style), {x});
        };
        PushCommand(command);
    }

    void ImguiContext::PushStyleVar(winrt::WsiuRenderer::ImGuiStyleVar const& style, float x, float y)
    {
        auto command = [style, x, y]
        {
            ImGui::PushStyleVar(static_cast<ImGuiStyleVar_>(style), {x, y});
        };
        PushCommand(command);
    }

    void ImguiContext::PopStyleVar()
    {
        auto command = []
        {
            ImGui::PopStyleVar();
        };
        PushCommand(command);
    }

    void ImguiContext::PopStyleVar(int count)
    {
        auto command = [count]
        {
            ImGui::PopStyleVar(count);
        };
        PushCommand(command);
    }

    void ImguiContext::PushStyleColor(winrt::WsiuRenderer::ImGuiCol const& col, float r, float g, float b, float a)
    {
        auto command = [col, r, g, b, a]
        {
            ImGui::PushStyleColor(static_cast<ImGuiCol_>(col), {r, g, b, a});
        };
        PushCommand(command);
    }

    void ImguiContext::PopStyleColor(int32_t count)
    {
        auto command = [count]
        {
            ImGui::PopStyleColor(count);
        };
        PushCommand(command);
    }

    void ImguiContext::PopStyleColor()
    {
        auto command = []
        {
            ImGui::PopStyleColor();
        };
        PushCommand(command);
    }

    void ImguiContext::Text(hstring const& text)
    {
        auto command = [text = winrt::to_string(text)]
        {
            ImGui::Text(text.c_str());
        };
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

    void ImguiContext::Selectable(hstring const&                                   label,
                                  bool                                             selected,
                                  winrt::WsiuRenderer::ImGuiSelectableFlags const& flags,
                                  winrt::WsiuRenderer::ButtonCallback const&       handle)
    {
        Selectable(label, selected, flags, 0, 0, handle);
    }

    void ImguiContext::Selectable(hstring const&                                   label,
                                  bool                                             selected,
                                  winrt::WsiuRenderer::ImGuiSelectableFlags const& flags,
                                  float                                            x,
                                  float                                            y,
                                  winrt::WsiuRenderer::ButtonCallback const&       handle)
    {
        auto command = [label = winrt::to_string(label), selected, flags, handle, x, y]() mutable
        {
            if (ImGui::Selectable(label.c_str(), selected, static_cast<ImGuiSelectableFlags_>(flags), {x, y}))
            {
                handle();
            }
        };
        PushCommand(command);
    }

    void ImguiContext::SettingFloat(
        float speed, float min, float max, hstring const& format, winrt::WsiuRenderer::ImGuiSliderFlags const& flags)
    {
        _floatSetting.Speed  = speed;
        _floatSetting.Min    = min;
        _floatSetting.Max    = max;
        _floatSetting.Format = winrt::to_string(format);
        _floatSetting.Flags  = flags;
    }

    void
    ImguiContext::DragFloat(hstring const& label, float val, winrt::WsiuRenderer::FloatChangedCallback const& handle)
    {
        auto command = [label = winrt::to_string(label), val, handle, this]() mutable
        {
            auto& setting = _floatSetting;
            ImGuiHelper::DragScalaWithCallback(label.c_str(),
                                               handle,
                                               &val,
                                               setting.Speed,
                                               &setting.Min,
                                               &setting.Max,
                                               setting.Format.c_str(),
                                               static_cast<::ImGuiSliderFlags>(setting.Flags));
        };
        PushCommand(command);
    }

    void ImguiContext::DragFloatN(hstring const&                                    label,
                                  array_view<float const>                           val,
                                  winrt::WsiuRenderer::FloatNChangedCallback const& handle)
    {
        if (4 < val.size())
            return;

        std::array<float, 4> temp{};
        std::copy(val.begin(), val.end(), temp.data());
        auto command = [label = winrt::to_string(label), temp, size = val.size(), handle, this]() mutable
        {
            auto& setting = _floatSetting;
            ImGuiHelper::DragScalaNWithCallback(label.c_str(),
                                                handle,
                                                size,
                                                temp.data(),
                                                setting.Speed,
                                                &setting.Min,
                                                &setting.Max,
                                                setting.Format.c_str(),
                                                static_cast<::ImGuiSliderFlags>(setting.Flags));
        };
        PushCommand(command);
    }

    void ImguiContext::SettingDouble(
        float speed, double min, double max, hstring const& format, winrt::WsiuRenderer::ImGuiSliderFlags const& flags)
    {
        _doubleSetting.Speed  = speed;
        _doubleSetting.Min    = min;
        _doubleSetting.Max    = max;
        _doubleSetting.Format = winrt::to_string(format);
        _doubleSetting.Flags  = flags;
    }

    void
    ImguiContext::DragDouble(hstring const& label, double val, winrt::WsiuRenderer::DoubleChangedCallback const& handle)
    {
        auto command = [label = winrt::to_string(label), val, handle, this]() mutable
        {
            auto& setting = _doubleSetting;
            ImGuiHelper::DragScalaWithCallback(label.c_str(),
                                               handle,
                                               &val,
                                               setting.Speed,
                                               &setting.Min,
                                               &setting.Max,
                                               setting.Format.c_str(),
                                               static_cast<::ImGuiSliderFlags>(setting.Flags));
        };
        PushCommand(command);
    }

    void ImguiContext::DragDoubleN(hstring const&                                     label,
                                   array_view<double const>                           val,
                                   winrt::WsiuRenderer::DoubleNChangedCallback const& handle)
    {
        if (4 < val.size())
            return;

        std::array<double, 4> temp{};
        std::copy(val.begin(), val.end(), temp.data());
        auto command = [label = winrt::to_string(label), temp, size = val.size(), handle, this]() mutable
        {
            auto& setting = _doubleSetting;
            ImGuiHelper::DragScalaNWithCallback(label.c_str(),
                                                handle,
                                                size,
                                                temp.data(),
                                                setting.Speed,
                                                &setting.Min,
                                                &setting.Max,
                                                setting.Format.c_str(),
                                                static_cast<::ImGuiSliderFlags>(setting.Flags));
        };
        PushCommand(command);
    }

    void ImguiContext::SettingUInt16(float                                        speed,
                                     uint16_t                                     min,
                                     uint16_t                                     max,
                                     hstring const&                               format,
                                     winrt::WsiuRenderer::ImGuiSliderFlags const& flags)
    {
        _uint16Setting.Speed  = speed;
        _uint16Setting.Min    = min;
        _uint16Setting.Max    = max;
        _uint16Setting.Format = winrt::to_string(format);
        _uint16Setting.Flags  = flags;
    }

    void ImguiContext::DragUInt16(hstring const&                                    label,
                                  uint16_t                                          val,
                                  winrt::WsiuRenderer::UInt16ChangedCallback const& handle)
    {
        auto command = [this, label = winrt::to_string(label), val, handle]() mutable
        {
            auto& setting = _uint16Setting;
            ImGuiHelper::DragScalaWithCallback(label.c_str(),
                                               handle,
                                               &val,
                                               setting.Speed,
                                               &setting.Min,
                                               &setting.Max,
                                               setting.Format.c_str(),
                                               static_cast<ImGuiSliderFlags_>(setting.Flags));
        };
        PushCommand(command);
    }

    void ImguiContext::DragUInt16N(hstring const&                                     label,
                                   array_view<uint16_t const>                         val,
                                   winrt::WsiuRenderer::UInt16NChangedCallback const& handle)
    {
        std::array<uint16_t, 4> temp{};
        std::copy(val.begin(), val.end(), temp.data());
        auto command = [this, label = winrt::to_string(label), size = val.size(), handle, temp]() mutable
        {
            auto& setting = _uint16Setting;
            ImGuiHelper::DragScalaNWithCallback(label.c_str(),
                                                handle,
                                                size,
                                                temp.data(),
                                                setting.Speed,
                                                &setting.Min,
                                                &setting.Max,
                                                setting.Format.c_str(),
                                                static_cast<ImGuiSliderFlags_>(setting.Flags));
        };
        PushCommand(command);
    }

    void ImguiContext::SettingUInt32(float                                        speed,
                                     uint32_t                                     min,
                                     uint32_t                                     max,
                                     hstring const&                               format,
                                     winrt::WsiuRenderer::ImGuiSliderFlags const& flags)
    {
        _uint32Setting.Speed  = speed;
        _uint32Setting.Min    = min;
        _uint32Setting.Max    = max;
        _uint32Setting.Format = winrt::to_string(format);
        _uint32Setting.Flags  = flags;
    }

    void ImguiContext::DragUInt32(hstring const&                                    label,
                                  uint32_t                                          val,
                                  winrt::WsiuRenderer::UInt32ChangedCallback const& handle)
    {
        auto command = [this, label = winrt::to_string(label), val, handle]() mutable
        {
            auto& setting = _uint32Setting;
            ImGuiHelper::DragScalaWithCallback(label.c_str(),
                                               handle,
                                               &val,
                                               setting.Speed,
                                               &setting.Min,
                                               &setting.Max,
                                               setting.Format.c_str(),
                                               static_cast<ImGuiSliderFlags_>(setting.Flags));
        };
        PushCommand(command);
    }

    void ImguiContext::DragUInt32N(hstring const&                                     label,
                                   array_view<uint32_t const>                         val,
                                   winrt::WsiuRenderer::UInt32NChangedCallback const& handle)
    {
        std::array<uint32_t, 4> temp{};
        std::copy(val.begin(), val.end(), temp.data());
        auto command = [this, label = winrt::to_string(label), size = val.size(), handle, temp]() mutable
        {
            auto& setting = _uint32Setting;
            ImGuiHelper::DragScalaNWithCallback(label.c_str(),
                                                handle,
                                                size,
                                                temp.data(),
                                                setting.Speed,
                                                &setting.Min,
                                                &setting.Max,
                                                setting.Format.c_str(),
                                                static_cast<ImGuiSliderFlags_>(setting.Flags));
        };
        PushCommand(command);
    }

    void ImguiContext::SettingUInt64(float                                        speed,
                                     uint64_t                                     min,
                                     uint64_t                                     max,
                                     hstring const&                               format,
                                     winrt::WsiuRenderer::ImGuiSliderFlags const& flags)
    {
        _uint64Setting.Speed  = speed;
        _uint64Setting.Min    = min;
        _uint64Setting.Max    = max;
        _uint64Setting.Format = winrt::to_string(format);
        _uint64Setting.Flags  = flags;
    }

    void ImguiContext::DragUInt64(hstring const&                                    label,
                                  uint64_t                                          val,
                                  winrt::WsiuRenderer::UInt64ChangedCallback const& handle)
    {
        auto command = [this, label = winrt::to_string(label), val, handle]() mutable
        {
            auto& setting = _uint64Setting;
            ImGuiHelper::DragScalaWithCallback(label.c_str(),
                                               handle,
                                               &val,
                                               setting.Speed,
                                               &setting.Min,
                                               &setting.Max,
                                               setting.Format.c_str(),
                                               static_cast<ImGuiSliderFlags_>(setting.Flags));
        };
        PushCommand(command);
    }

    void ImguiContext::DragUInt64N(hstring const&                                     label,
                                   array_view<uint64_t const>                         val,
                                   winrt::WsiuRenderer::UInt64NChangedCallback const& handle)
    {
        std::array<uint64_t, 4> temp{};
        std::copy(val.begin(), val.end(), temp.data());
        auto command = [this, label = winrt::to_string(label), size = val.size(), handle, temp]() mutable
        {
            auto& setting = _uint64Setting;
            ImGuiHelper::DragScalaNWithCallback(label.c_str(),
                                                handle,
                                                size,
                                                temp.data(),
                                                setting.Speed,
                                                &setting.Min,
                                                &setting.Max,
                                                setting.Format.c_str(),
                                                static_cast<ImGuiSliderFlags_>(setting.Flags));
        };
        PushCommand(command);
    }

    void ImguiContext::SettingInt16(float                                        speed,
                                    int16_t                                      min,
                                    int16_t                                      max,
                                    hstring const&                               format,
                                    winrt::WsiuRenderer::ImGuiSliderFlags const& flags)
    {
        _int64Setting.Speed  = speed;
        _int64Setting.Min    = min;
        _int64Setting.Max    = max;
        _int64Setting.Format = winrt::to_string(format);
        _int64Setting.Flags  = flags;
    }

    void
    ImguiContext::DragInt16(hstring const& label, int16_t val, winrt::WsiuRenderer::Int16ChangedCallback const& handle)
    {
        auto command = [this, label = winrt::to_string(label), val, handle]() mutable
        {
            auto& setting = _int16Setting;
            ImGuiHelper::DragScalaWithCallback(label.c_str(),
                                               handle,
                                               &val,
                                               setting.Speed,
                                               &setting.Min,
                                               &setting.Max,
                                               setting.Format.c_str(),
                                               static_cast<ImGuiSliderFlags_>(setting.Flags));
        };
        PushCommand(command);
    }

    void ImguiContext::DragInt16N(hstring const&                                    label,
                                  array_view<int16_t const>                         val,
                                  winrt::WsiuRenderer::Int16NChangedCallback const& handle)
    {
        std::array<int16_t, 4> temp{};
        std::copy(val.begin(), val.end(), temp.data());
        auto command = [this, label = winrt::to_string(label), size = val.size(), handle, temp]() mutable
        {
            auto& setting = _int16Setting;
            ImGuiHelper::DragScalaNWithCallback(label.c_str(),
                                                handle,
                                                size,
                                                temp.data(),
                                                setting.Speed,
                                                &setting.Min,
                                                &setting.Max,
                                                setting.Format.c_str(),
                                                static_cast<ImGuiSliderFlags_>(setting.Flags));
        };
        PushCommand(command);
    }

    void ImguiContext::SettingInt32(float                                        speed,
                                    int32_t                                      min,
                                    int32_t                                      max,
                                    hstring const&                               format,
                                    winrt::WsiuRenderer::ImGuiSliderFlags const& flags)
    {
        _int32Setting.Speed  = speed;
        _int32Setting.Min    = min;
        _int32Setting.Max    = max;
        _int32Setting.Format = winrt::to_string(format);
        _int32Setting.Flags  = flags;
    }

    void
    ImguiContext::DragInt32(hstring const& label, int32_t val, winrt::WsiuRenderer::Int32ChangedCallback const& handle)
    {
        auto command = [this, label = winrt::to_string(label), val, handle]() mutable
        {
            auto& setting = _int32Setting;
            ImGuiHelper::DragScalaWithCallback(label.c_str(),
                                               handle,
                                               &val,
                                               setting.Speed,
                                               &setting.Min,
                                               &setting.Max,
                                               setting.Format.c_str(),
                                               static_cast<ImGuiSliderFlags_>(setting.Flags));
        };
        PushCommand(command);
    }

    void ImguiContext::DragInt32N(hstring const&                                    label,
                                  array_view<int32_t const>                         val,
                                  winrt::WsiuRenderer::Int32NChangedCallback const& handle)
    {
        std::array<int32_t, 4> temp{};
        std::copy(val.begin(), val.end(), temp.data());
        auto command = [this, label = winrt::to_string(label), size = val.size(), handle, temp]() mutable
        {
            auto& setting = _int32Setting;
            ImGuiHelper::DragScalaNWithCallback(label.c_str(),
                                                handle,
                                                size,
                                                temp.data(),
                                                setting.Speed,
                                                &setting.Min,
                                                &setting.Max,
                                                setting.Format.c_str(),
                                                static_cast<ImGuiSliderFlags_>(setting.Flags));
        };
        PushCommand(command);
    }

    void ImguiContext::SettingInt64(float                                        speed,
                                    int64_t                                      min,
                                    int64_t                                      max,
                                    hstring const&                               format,
                                    winrt::WsiuRenderer::ImGuiSliderFlags const& flags)
    {
        _int64Setting.Speed  = speed;
        _int64Setting.Min    = min;
        _int64Setting.Max    = max;
        _int64Setting.Format = winrt::to_string(format);
        _int64Setting.Flags  = flags;
    }

    void
    ImguiContext::DragInt64(hstring const& label, int64_t val, winrt::WsiuRenderer::Int64ChangedCallback const& handle)
    {
        auto command = [this, label = winrt::to_string(label), val, handle]() mutable
        {
            auto& setting = _int64Setting;
            ImGuiHelper::DragScalaWithCallback(label.c_str(),
                                               handle,
                                               &val,
                                               setting.Speed,
                                               &setting.Min,
                                               &setting.Max,
                                               setting.Format.c_str(),
                                               static_cast<ImGuiSliderFlags_>(setting.Flags));
        };
        PushCommand(command);
    }

    void ImguiContext::DragInt64N(hstring const&                                    label,
                                  array_view<int64_t const>                         val,
                                  winrt::WsiuRenderer::Int64NChangedCallback const& handle)
    {
        std::array<int64_t, 4> temp{};
        std::copy(val.begin(), val.end(), temp.data());
        auto command = [this, label = winrt::to_string(label), size = val.size(), handle, temp]() mutable
        {
            auto& setting = _int64Setting;
            ImGuiHelper::DragScalaNWithCallback(label.c_str(),
                                                handle,
                                                size,
                                                temp.data(),
                                                setting.Speed,
                                                &setting.Min,
                                                &setting.Max,
                                                setting.Format.c_str(),
                                                static_cast<ImGuiSliderFlags_>(setting.Flags));
        };
        PushCommand(command);
    }
} // namespace winrt::WsiuRenderer::implementation
