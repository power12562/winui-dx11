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

    private:
        ComPtr<ID3D11Device5>           _device;
        ComPtr<ID3D11DeviceContext4>    _deviceContext;
        ComPtr<IDXGISwapChain1>        _swapChain;
        ComPtr<ID3D11RenderTargetView> _renderTargetView;
    };
}

namespace winrt::WsiuRenderer::factory_implementation
{
    struct EngineCore : EngineCoreT<EngineCore, implementation::EngineCore>
    {
    };
}
