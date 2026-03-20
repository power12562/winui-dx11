#pragma once
#include "ImguiContext.g.h"
#include "slot_pool.h"
#include "imgui_helper.h"

namespace winrt::WsiuRenderer::implementation
{
    struct ImguiContext : ImguiContextT<ImguiContext>
    {
        using EngineCore = winrt::WsiuRenderer::EngineCore;
        template<typename _scala_type>
        struct ScalaSetting
        {
            ScalaSetting(const char* format) : Format(format) {}

            using scala_type = _scala_type;
            float Speed = 1.0f;
            scala_type Min{};
            scala_type Max{};
            std::string Format;
            winrt::WsiuRenderer::ImGuiSliderFlags Flags = winrt::WsiuRenderer::ImGuiSliderFlags::None;
        };
        using FloatSetting = ScalaSetting<float>;
        using DoubleSetting = ScalaSetting<double>;
        using UInt16Setting = ScalaSetting<uint16_t>;
        using UInt32Setting = ScalaSetting<uint32_t>;
        using UInt64Setting = ScalaSetting<uint64_t>;
        using Int16Setting = ScalaSetting<int16_t>;
        using Int32Setting = ScalaSetting<int32_t>;
        using Int64Setting = ScalaSetting<int64_t>;
        inline static FloatSetting  _floatSetting{ImGuiHelper::format_float};
        inline static DoubleSetting _doubleSetting{ImGuiHelper::format_double};
        inline static UInt16Setting _uint16Setting{ImGuiHelper::format_uint16};
        inline static UInt32Setting _uint32Setting{ImGuiHelper::format_uint32};
        inline static UInt64Setting _uint64Setting{ImGuiHelper::format_uint64};
        inline static Int16Setting _int16Setting{ImGuiHelper::format_int16};
        inline static Int32Setting _int32Setting{ImGuiHelper::format_int32};
        inline static Int64Setting _int64Setting{ImGuiHelper::format_int64};
    public:
        ImguiContext(EngineCore const& engineCore);
        ~ImguiContext() override;

        static hstring SaveIniSettingsToMemory();
        static void LoadIniSettingsFromMemory(hstring const& data);

        void InitializeCommands(hstring const& title);
        void InitializeWindow(hstring const& title);
        void InitializeWindowClosable(hstring const& title);

        uint64_t GetWindowID() const { return _windowID; }
        bool GetActive() const;
        void SetActive(bool active);
        void SetTitle(hstring const& title) const;

        void Separator();
        void SeparatorText(hstring const& text);

        void PushID(uint32_t id);
        void PopID();

        void BeginDisabled();
        void EndDisabled();

        void BeginMainMenuBar();
        void EndMainMenuBar();

        void BeginMenu(hstring const& label);
        void BeginMenu(hstring const& label, bool enabled);
        void EndMenu();

        void MenuItem(hstring const& label, winrt::WsiuRenderer::ButtonCallback const& handle);
        void MenuItem(hstring const& label, bool selected, winrt::WsiuRenderer::ButtonCallback const& handle);
        void MenuItem(hstring const& label, bool selected, bool enabled, winrt::WsiuRenderer::ButtonCallback const& handle);

        void TreeNodeEx(hstring const& label, winrt::WsiuRenderer::ImGuiTreeNodeFlags const& flags);
        void TreePop();

        void PushStyleVar(winrt::WsiuRenderer::ImGuiStyleVar const& style, float x);
        void PushStyleVar(winrt::WsiuRenderer::ImGuiStyleVar const& style, float x, float y);
        void PopStyleVar();
        void PopStyleVar(int count);

        void PushStyleColor(winrt::WsiuRenderer::ImGuiCol const& col, float r, float g, float b, float a);
        void PopStyleColor(int32_t count);
        void PopStyleColor();

        void Text(hstring const& text);
        void Button(hstring const& label, winrt::WsiuRenderer::ButtonCallback const& handle);
        void Button(hstring const& label, float x, float y, winrt::WsiuRenderer::ButtonCallback const& handle);
        void Selectable(hstring const& label, bool selected, winrt::WsiuRenderer::ImGuiSelectableFlags const& flags,
                        winrt::WsiuRenderer::ButtonCallback const& handle);
        void Selectable(hstring const& label, bool selected, winrt::WsiuRenderer::ImGuiSelectableFlags const& flags,
                        float x, float y, winrt::WsiuRenderer::ButtonCallback const& handle);

        static void SettingFloat(float speed, float min, float max, hstring const& format,
                          winrt::WsiuRenderer::ImGuiSliderFlags const& flags);
        void DragFloat(hstring const& label, float val, winrt::WsiuRenderer::FloatChangedCallback const& handle);
        void DragFloatN(hstring const& label, array_view<float const> val,
                        winrt::WsiuRenderer::FloatNChangedCallback const& handle);

        static void SettingDouble(float speed, double min, double max, hstring const& format,
                           winrt::WsiuRenderer::ImGuiSliderFlags const& flags);
        void DragDouble(hstring const& label, double val, winrt::WsiuRenderer::DoubleChangedCallback const& handle);
        void DragDoubleN(hstring const& label, array_view<double const> val,
                         winrt::WsiuRenderer::DoubleNChangedCallback const& handle);

        static void SettingUInt16(float speed, uint16_t min, uint16_t max, hstring const& format,
                                  winrt::WsiuRenderer::ImGuiSliderFlags const& flags);
        void DragUInt16(hstring const& label, uint16_t val, winrt::WsiuRenderer::UInt16ChangedCallback const& handle);
        void DragUInt16N(hstring const& label, array_view<uint16_t const> val,
                         winrt::WsiuRenderer::UInt16NChangedCallback const& handle);

        static void SettingUInt32(float speed, uint32_t min, uint32_t max, hstring const& format,
                                  winrt::WsiuRenderer::ImGuiSliderFlags const& flags);
        void DragUInt32(hstring const& label, uint32_t val, winrt::WsiuRenderer::UInt32ChangedCallback const& handle);
        void DragUInt32N(hstring const& label, array_view<uint32_t const> val,
                         winrt::WsiuRenderer::UInt32NChangedCallback const& handle);

        static void SettingUInt64(float speed, uint64_t min, uint64_t max, hstring const& format,
                                  winrt::WsiuRenderer::ImGuiSliderFlags const& flags);
        void DragUInt64(hstring const& label, uint64_t val, winrt::WsiuRenderer::UInt64ChangedCallback const& handle);
        void DragUInt64N(hstring const& label, array_view<uint64_t const> val,
                         winrt::WsiuRenderer::UInt64NChangedCallback const& handle);

        static void SettingInt16(float speed, int16_t min, int16_t max, hstring const& format,
                                 winrt::WsiuRenderer::ImGuiSliderFlags const& flags);
        void DragInt16(hstring const& label, int16_t val, winrt::WsiuRenderer::Int16ChangedCallback const& handle);
        void DragInt16N(hstring const& label, array_view<int16_t const> val,
                        winrt::WsiuRenderer::Int16NChangedCallback const& handle);

        static void SettingInt32(float speed, int32_t min, int32_t max, hstring const& format,
                                 winrt::WsiuRenderer::ImGuiSliderFlags const& flags);
        void DragInt32(hstring const& label, int32_t val, winrt::WsiuRenderer::Int32ChangedCallback const& handle);
        void DragInt32N(hstring const& label, array_view<int32_t const> val,
                        winrt::WsiuRenderer::Int32NChangedCallback const& handle);

        static void SettingInt64(float speed, int64_t min, int64_t max, hstring const& format,
                                 winrt::WsiuRenderer::ImGuiSliderFlags const& flags);
        void DragInt64(hstring const& label, int64_t val, winrt::WsiuRenderer::Int64ChangedCallback const& handle);
        void DragInt64N(hstring const& label, array_view<int64_t const> val,
                        winrt::WsiuRenderer::Int64NChangedCallback const& handle);
    private:
        using Commands = std::vector<std::function<void()>>;
        using CommandsStack = std::vector<size_t>;
        using CommandsStackCounter = slot_pool<size_t>;
        EngineCore _engineCore;
        uint64_t _windowID   = (std::numeric_limits<uint64_t>::max)();
        size_t  _stackDepth   = 0;
        Commands _commands;
        CommandsStack _commandsStack;
        CommandsStackCounter _commandsStackCounter;
        size_t _skipCommandCount = 0;

        void DrawCommands();
        void ClearCommandsStack();
        void SkipCommand(size_t counterId);
        void PushCommandStack(size_t counterId);
        void PopCommandStack();    
        template<typename _command> 
        void PushCommand(_command&& command)
        {
            for (auto& counterID : _commandsStack)
            {
                auto& count = _commandsStackCounter.at(counterID);
                ++count;
            }
            return _commands.push_back(std::forward<_command>(command));
        }
    };
}

namespace winrt::WsiuRenderer::factory_implementation
{
    struct ImguiContext : ImguiContextT<ImguiContext, implementation::ImguiContext>
    {
    };
}
