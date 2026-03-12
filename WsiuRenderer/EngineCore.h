#pragma once
#include "EngineCore.g.h"

namespace winrt::WsiuRenderer::implementation
{
    struct EngineCore : EngineCoreT<EngineCore>
    {
        EngineCore() = default;
        void Initialize(uint64_t windowHandle, winrt::Microsoft::UI::Xaml::Controls::SwapChainPanel const& panel);
        void Run();
        bool VSync() const;
        void VSync(bool value);
        void Quit() { _isRun = false; }

    private:
        static constexpr UINT_PTR IID = 1;
        using DispatcherQueue         = Microsoft::UI::Dispatching::DispatcherQueue;

        bool SetSubclass(uint64_t windowHandle);
        bool CreateDevice();
        bool CreateSwapChain(winrt::Microsoft::UI::Xaml::Controls::SwapChainPanel const& panel);
        void Tick();

        void Clear();
        void BeginImgui();
        void Update();
        void Render();
        void EndImgui();
        void Flip();

        void Finalize();
        
        HWND                           _hwnd;
        DispatcherQueue                _dispatcherQueue = nullptr;
        bool                           _isRun = true;
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
