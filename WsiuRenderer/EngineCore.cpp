#include "pch.h"
#include "EngineCore.h"
#include <microsoft.ui.xaml.media.dxinterop.h>
#include "EngineCore.g.cpp"

namespace winrt::WsiuRenderer::implementation
{
    void EngineCore::Initialize(winrt::Microsoft::UI::Xaml::Controls::SwapChainPanel const& panel)
    {
        if (CreateDevice() == false)
            throw std::runtime_error("Create Device Failed.");

        if (CreateSwapChain(panel) == false)
            throw std::runtime_error("Create Device Failed.");
    }

    void EngineCore::Update() {}

    void EngineCore::Render()
    {
        constexpr float clearColor[] = {0.1f, 0.1f, 0.1f, 1.0f};
        _deviceContext->ClearRenderTargetView(_backbufferRTV.Get(), clearColor);
        _swapChain->Present(_vSync ? 1 : 0, 0);
    }

    bool EngineCore::VSync() const
    {
        return _vSync;
    }

    void EngineCore::VSync(bool value) 
    {
        _vSync = value;
    }

    bool EngineCore::CreateDevice()
    {
        UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;
#ifdef _DEBUG
        creationFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif

        D3D_FEATURE_LEVEL featureLevels[] = {D3D_FEATURE_LEVEL_11_1};

        ComPtr<ID3D11Device>        baseDevice;
        ComPtr<ID3D11DeviceContext> baseContext;

        HRESULT hr =
            D3D11CreateDevice(nullptr, // 기본 어댑터
                              D3D_DRIVER_TYPE_HARDWARE, nullptr, creationFlags, featureLevels, ARRAYSIZE(featureLevels),
                              D3D11_SDK_VERSION, baseDevice.GetAddressOf(), nullptr, baseContext.GetAddressOf());
        if (FAILED(hr))
            return false;

        hr = baseDevice.As(&_device);
        if (FAILED(hr))
            return false;

        hr = baseContext.As(&_deviceContext);
        if (FAILED(hr))
            return false;

        return true;
    }

    bool EngineCore::CreateSwapChain(winrt::Microsoft::UI::Xaml::Controls::SwapChainPanel const& panel)
    {
        ComPtr<IDXGIDevice3> dxgiDevice;
        HRESULT              hr = _device.As(&dxgiDevice);
        if (FAILED(hr))
            return false;

        ComPtr<IDXGIAdapter> dxgiAdapter;
        hr = dxgiDevice->GetAdapter(&dxgiAdapter);
        if (FAILED(hr))
            return false;

        ComPtr<IDXGIFactory2> dxgiFactory;
        hr = dxgiAdapter->GetParent(IID_PPV_ARGS(&dxgiFactory));
        if (FAILED(hr))
            return false;

        UINT width  = GetSystemMetrics(SM_CXSCREEN);
        UINT height = GetSystemMetrics(SM_CYSCREEN);

        DXGI_SWAP_CHAIN_DESC1 swapChainDesc = {0};
        swapChainDesc.Width                 = width;
        swapChainDesc.Height                = height;
        swapChainDesc.Format                = DXGI_FORMAT_B8G8R8A8_UNORM;
        swapChainDesc.Stereo                = false;
        swapChainDesc.SampleDesc.Count      = 1;
        swapChainDesc.SampleDesc.Quality    = 0;
        swapChainDesc.BufferUsage           = DXGI_USAGE_RENDER_TARGET_OUTPUT;
        swapChainDesc.BufferCount           = 2;
        swapChainDesc.Scaling               = DXGI_SCALING_STRETCH;
        swapChainDesc.SwapEffect            = DXGI_SWAP_EFFECT_FLIP_DISCARD;

        hr = dxgiFactory->CreateSwapChainForComposition(_device.Get(), &swapChainDesc, nullptr,
                                                        _swapChain.GetAddressOf());
        if (FAILED(hr))
            return false;

        ComPtr<ID3D11Texture2D> backBuffer;
        hr = _swapChain->GetBuffer(0, IID_PPV_ARGS(backBuffer.GetAddressOf()));
        if (FAILED(hr))
            return false;

        hr = _device->CreateRenderTargetView(backBuffer.Get(), nullptr, _backbufferRTV.GetAddressOf());
        if (FAILED(hr))
            return false;

        auto panelNative = panel.try_as<ISwapChainPanelNative>();
        if (panelNative == nullptr)
            return false;

        hr = panelNative->SetSwapChain(_swapChain.Get());
        if (FAILED(hr))
            return false;

        return true;
    }
} // namespace winrt::WsiuRenderer::implementation
