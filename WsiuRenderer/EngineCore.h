#pragma once
#include "EngineCore.g.h"

namespace winrt::WsiuRenderer::implementation
{
    struct EngineCore : EngineCoreT<EngineCore>
    {
        EngineCore() = default;

        void Initialize();
        void Update();
        void Render();
    };
}

namespace winrt::WsiuRenderer::factory_implementation
{
    struct EngineCore : EngineCoreT<EngineCore, implementation::EngineCore>
    {
    };
}
