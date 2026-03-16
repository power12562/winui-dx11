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
            winrt::WsiuRenderer::ImGuiSliderFlags Flags  = winrt::WsiuRenderer::ImGuiSliderFlags::None;
        };
        using FloatSetting = ScalaSetting<float>;
        using DoubleSetting = ScalaSetting<double>;
    public:
        ImguiContext(EngineCore const& engineCore);
        ~ImguiContext() override;

        void InitializeWindow(hstring const& title);
        uint64_t GetWindowID() const { return _windowID; }
        void SetTitle(hstring const& title) const;

        void PushID(uint32_t id);
        void PopID();

        void BeginDisabled();
        void EndDisabled();

        void PushStyleVar(winrt::WsiuRenderer::ImGuiStyleVar const& style, float x);
        void PushStyleVar(winrt::WsiuRenderer::ImGuiStyleVar const& style, float x, float y);
        void PopStyleVar();
        void PopStyleVar(int count);

        void Text(hstring const& text);

        void SettingFloat(float speed, float min, float max, hstring const& format,
                          winrt::WsiuRenderer::ImGuiSliderFlags const& flags);
        void DragFloat(hstring const& label, float val, winrt::WsiuRenderer::FloatChangedCallback const& handle);

        void SettingDouble(float speed, double min, double max, hstring const& format,
                           winrt::WsiuRenderer::ImGuiSliderFlags const& flags);
        void DragDouble(hstring const& label, double val, winrt::WsiuRenderer::DoubleChangedCallback const& handle);
    protected:
        EngineCore _engineCore;

    private:
        void DrawCommands();

        uint64_t _windowID = (std::numeric_limits<uint64_t>::max)();
        std::vector<std::function<void()>> _commands;

        FloatSetting _floatSetting;
        DoubleSetting _doubleSetting;
    };
}

namespace winrt::WsiuRenderer::factory_implementation
{
    struct ImguiContext : ImguiContextT<ImguiContext, implementation::ImguiContext>
    {
    };
}
