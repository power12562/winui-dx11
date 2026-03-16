#pragma once
#include "ImguiContext.g.h"

namespace winrt::WsiuRenderer::implementation
{
    struct ImguiContext : ImguiContextT<ImguiContext>
    {
        using EngineCore = winrt::WsiuRenderer::EngineCore;
    public:
        ImguiContext(EngineCore const& engineCore);
        ~ImguiContext() override;

        void InitializeWindow(hstring const& title);
        uint64_t GetWindowID() const { return _windowID; }
        void SetTitle(hstring const& title) const;

        void PushID(uint32_t id);
        void PopID();
        void Text(hstring const& text);

    protected:
        EngineCore _engineCore;

    private:
        void DrawCommands();

        uint64_t _windowID = (std::numeric_limits<uint64_t>::max)();
        std::vector<std::function<void()>> _beginCommands;
        std::vector<std::function<void()>> _endCommands;
    };
}

namespace winrt::WsiuRenderer::factory_implementation
{
    struct ImguiContext : ImguiContextT<ImguiContext, implementation::ImguiContext>
    {
    };
}
