#pragma once
#include "ImguiContext.g.h"

namespace winrt::WsiuRenderer::implementation
{
    struct ImguiContext : ImguiContextT<ImguiContext>
    {
        using EngineCore = winrt::WsiuRenderer::EngineCore;
        template<typename _scala_type>
        struct ScalaSetting
        {
            using scala_type = _scala_type;
            float Speed = 1.0f;
            scala_type Min{};
            scala_type Max{};
            std::string Format = "%.3f";
            winrt::WsiuRenderer::ImGuiSliderFlags Flags = winrt::WsiuRenderer::ImGuiSliderFlags::None;
        };
        using FloatSetting = ScalaSetting<float>;
        using DoubleSetting = ScalaSetting<double>;
        inline static FloatSetting  _floatSetting;
        inline static DoubleSetting _doubleSetting;
    public:
        ImguiContext(EngineCore const& engineCore);
        ~ImguiContext() override;

        void InitializeWindow(hstring const& title);
        void InitializeWindowClosable(hstring const& title);
        uint64_t GetWindowID() const { return _windowID; }
        void SetActive(bool active);
        void SetTitle(hstring const& title) const;

        void PushID(uint32_t id);
        void PopID();

        void BeginDisabled();
        void EndDisabled();

        void TreeNodeEx(hstring const& label, winrt::WsiuRenderer::ImGuiTreeNodeFlags const& flags);
        void TreePop();

        void PushStyleVar(winrt::WsiuRenderer::ImGuiStyleVar const& style, float x);
        void PushStyleVar(winrt::WsiuRenderer::ImGuiStyleVar const& style, float x, float y);
        void PopStyleVar();
        void PopStyleVar(int count);

        void Text(hstring const& text);
        void Button(hstring const& label, winrt::WsiuRenderer::ButtonCallback const& handle);
        void Button(hstring const& label, float x, float y, winrt::WsiuRenderer::ButtonCallback const& handle);

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

    private:
        using Commands = std::vector<std::function<void()>>;
        using CommandsStack = std::vector<Commands>;
        using CommandsStackIter = std::vector<std::pair<Commands::iterator, Commands::iterator>>;
        EngineCore _engineCore;
        uint64_t _windowID = (std::numeric_limits<uint64_t>::max)();
        CommandsStackIter _commandsStackIter;
        size_t _iterIndex = 0;
        CommandsStack _commandsStack;

        void DrawCommands();
        void PushCommandsStack() { ++_iterIndex; }
        void PopCommandStack() { --_iterIndex; }
        template<typename _command> 
        void PushCommand(_command&& command)
        {
            if (_commandsStack.size() <= _iterIndex)
            {
                _commandsStack.emplace_back();
            }
            return _commandsStack[_iterIndex].push_back(std::forward<_command>(command));
        }
    };
}

namespace winrt::WsiuRenderer::factory_implementation
{
    struct ImguiContext : ImguiContextT<ImguiContext, implementation::ImguiContext>
    {
    };
}
