#include "pch.h"
#include "EngineCore.h"
#include "EngineCore.g.cpp"

namespace winrt::WsiuRenderer::implementation
{
	void EngineCore::Initialize()
	{
		if (CreateDevice() == false)
			throw std::runtime_error("Create Device Failed.");

		if (CreateSwapChain() == false)
			throw std::runtime_error("Create Device Failed.");
	}

    void EngineCore::Update()
    {
       
    }

    void EngineCore::Render()
    {
      
    }
    bool EngineCore::CreateDevice()
    {
        UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;
#ifdef _DEBUG
        creationFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif

        D3D_FEATURE_LEVEL featureLevels[] = { D3D_FEATURE_LEVEL_11_1 };

        ComPtr<ID3D11Device> baseDevice;
        ComPtr<ID3D11DeviceContext> baseContext;

        HRESULT hr = D3D11CreateDevice(
            nullptr,                    // 기본 어댑터
            D3D_DRIVER_TYPE_HARDWARE,
            nullptr,
            creationFlags,
            featureLevels,
            ARRAYSIZE(featureLevels),
            D3D11_SDK_VERSION,
            baseDevice.GetAddressOf(),
            nullptr,
            baseContext.GetAddressOf()
        );
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
    bool EngineCore::CreateSwapChain()
    {
        ComPtr<IDXGIDevice3> dxgiDevice;
        HRESULT hr = _device.As(&dxgiDevice);
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

        UINT width = GetSystemMetrics(SM_CXSCREEN);
        UINT height = GetSystemMetrics(SM_CYSCREEN);
      
        DXGI_SWAP_CHAIN_DESC1 swapChainDesc = { 0 };
        swapChainDesc.Width = width;
        swapChainDesc.Height = height;
        swapChainDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
        swapChainDesc.Stereo = false;
        swapChainDesc.SampleDesc.Count = 1;               
        swapChainDesc.SampleDesc.Quality = 0;
        swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
        swapChainDesc.BufferCount = 2;                     
        swapChainDesc.Scaling = DXGI_SCALING_STRETCH;      
        swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;

        hr = dxgiFactory->CreateSwapChainForComposition(
            _device.Get(),
            &swapChainDesc,
            nullptr,
            _swapChain.GetAddressOf()
        );
        if (FAILED(hr))
            return false;

        return true;
    }
}
