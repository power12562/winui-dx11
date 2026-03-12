#include "pch.h"
#include "EngineCore.h"
#include "EngineCore.g.cpp"

namespace winrt::WsiuRenderer::implementation
{
    void EngineCore::Initialize()
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

        if (SUCCEEDED(hr)) 
        {
            baseDevice.As(&_device);
            baseContext.As(&_deviceContext);
        }
    }

    void EngineCore::Update()
    {
       
    }

    void EngineCore::Render()
    {
      
    }
}
