#pragma once
#include "EngineCore.g.h"

namespace winrt::WsiuRenderer::implementation
{
    struct EngineCore : EngineCoreT<EngineCore>
    {
        EngineCore() = default;
        void Initialize(winrt::Microsoft::UI::Xaml::Controls::SwapChainPanel const& panel);
        void Update();
        void Render();
        bool VSync() const;
        void VSync(bool value);

    private:
        bool CreateDevice();
        bool CreateSwapChain(winrt::Microsoft::UI::Xaml::Controls::SwapChainPanel const& panel);

        bool                           _vSync = true;
        ComPtr<ID3D11Device5>          _device;
        ComPtr<ID3D11DeviceContext4>   _deviceContext;
        ComPtr<IDXGISwapChain1>        _swapChain;
        ComPtr<ID3D11RenderTargetView> _backbufferRTV;
    };
} // namespace winrt::WsiuRenderer::implementation

namespace winrt::WsiuRenderer::factory_implementation
{
    struct EngineCore : EngineCoreT<EngineCore, implementation::EngineCore>
    {
    };
} // namespace winrt::WsiuRenderer::factory_implementation
