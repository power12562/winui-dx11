#include "pch.h"
#include "EngineCore.h"
#include <microsoft.ui.xaml.media.dxinterop.h>
#include "EngineCore.g.cpp"
#include "imguicommons.h"
#include <commctrl.h>
#pragma comment(lib, "comctl32.lib")

extern IMGUI_IMPL_API LRESULT ImGui_ImplWin32_WndProcHandler(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);

namespace winrt::WsiuRenderer::implementation
{
    static LRESULT WinAppProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam, UINT_PTR, DWORD_PTR dwRefData)
    {
        if (ImGui_ImplWin32_WndProcHandler(hWnd, msg, wParam, lParam))
            return 1;

        EngineCore* core = reinterpret_cast<EngineCore*>(dwRefData);
        switch (msg)
        {
        case WM_INPUT:
            core->InputSystem.RawInputProcessing((HRAWINPUT)lParam);
            return 0; 
        case WM_SIZE:
            break;
        case WM_DESTROY:
            core->Quit();
            PostQuitMessage(0);
            return 0;
        }
        return DefSubclassProc(hWnd, msg, wParam, lParam);
    }

    void EngineCore::Initialize(uint64_t windowHandle, winrt::Microsoft::UI::Xaml::Controls::SwapChainPanel const& panel)
    {
        if (SetHWND(windowHandle) == false)
            throw std::runtime_error("Set HWND Failed.");

        if (CreateDevice() == false)
            throw std::runtime_error("Create Device Failed.");

        if (CreateSwapChain(panel) == false)
            throw std::runtime_error("Create Device Failed.");

        if (InputSystem.RawInputRegister(_hwnd) == false)
            throw std::runtime_error("Register RawInput Failed.");
    }

    void EngineCore::BeginFrame()
    {
        if (_isRun == false)
            return;

        Clear();
        BeginImgui();
    }

    void EngineCore::Tick()
    {
        if (_isRun == false)
            return;

        Update();
        Render();
    }

    void EngineCore::EndFrame()
    {
        if (_isRun == false)
            return;

        EndImgui();
        Flip();
        InputSystem.EndFrame();
    }


    void EngineCore::Clear() 
    {
        constexpr float clearColor[] = {0.0f, 1.0f, 1.0f, 1.0f};
        auto& backBuffer = GetBackBufferRTV();
        _deviceContext->ClearRenderTargetView(backBuffer.Get(), clearColor);
    }

    void EngineCore::BeginImgui() 
    {
        static InputSystem::MouseInputState prevMouseState{};
        static InputSystem::KeyboardInputState prevKeyboardState{};
        ImGuiIO& io = ImGui::GetIO();
        const auto& mouseState = InputSystem.MouseState;

        if (prevMouseState.IsLeftDown != mouseState.IsLeftDown)
            io.AddMouseButtonEvent(ImGuiMouseButton_Left, mouseState.IsLeftDown);
        if (prevMouseState.IsRightDown != mouseState.IsRightDown)
            io.AddMouseButtonEvent(ImGuiMouseButton_Right, mouseState.IsRightDown);
        if (prevMouseState.IsMiddleDown != mouseState.IsMiddleDown)
            io.AddMouseButtonEvent(ImGuiMouseButton_Middle, mouseState.IsMiddleDown);
        if (mouseState.WheelDelta != 0)
        {
            float wheel = (float)mouseState.WheelDelta / (float)WHEEL_DELTA;
            io.AddMouseWheelEvent(0.0f, wheel);
        }

        BYTE keyboardState[256];
        ::GetKeyboardState(keyboardState); 
        uint32_t* pBits = reinterpret_cast<uint32_t*>(&InputSystem.KeyboardState);
        uint32_t* pLastBits = reinterpret_cast<uint32_t*>(&prevKeyboardState);
        for (int i = 0; i < 256; ++i)
        {
            int  index  = i / 32;
            int  bit    = i % 32;

            bool isDown  = (pBits[index] & (1u << bit)) != 0;
            bool wasDown = (pLastBits[index] & (1u << bit)) != 0;

            if (isDown != wasDown)
            {
                ImGuiKey imguiKey = VirtualKeyToImGuiKey(i);
                if (imguiKey != ImGuiKey_None)
                {
                    io.AddKeyEvent(imguiKey, isDown);
                }
            }

            if (isDown != wasDown && isDown)
            {
                wchar_t buffer[5];
                int result = ToUnicode((UINT)i, i / 32, keyboardState, buffer, 4, 0);
                if (result > 0)
                {
                    for (int n = 0; n < result; n++)
                    {
                        io.AddInputCharacter(buffer[n]);
                    }
                }
            }
        }
        prevMouseState = mouseState;
        prevKeyboardState = InputSystem.KeyboardState;

        ImGui_ImplDX11_NewFrame();
        ImGui_ImplWin32_NewFrame();
        ImGui::NewFrame();
        ImGui::DockSpaceOverViewport();
    }

    void EngineCore::Update() 
    {
        ImGui::ShowDemoWindow(); // Show demo window! :)
    }

    void EngineCore::Render()
    {

    }

    void EngineCore::EndImgui() 
    {
        auto& backBuffer = GetBackBufferRTV();
        _deviceContext->OMSetRenderTargets(1, backBuffer.GetAddressOf(), nullptr);
        ImGui::Render();
        ImGui_ImplDX11_RenderDrawData(ImGui::GetDrawData());
    }

    void EngineCore::Flip() 
    {
        _swapChain->Present(_vSync ? 1 : 0, 0);
    }

    void EngineCore::Finalize() 
    { 
        ImGui_ImplDX11_Shutdown();
        ImGui_ImplWin32_Shutdown();
        ImGui::DestroyContext();

        _randerTargetViews.clear();
        _swapChain.Reset();
        _deviceContext->ClearState();
        _deviceContext->Flush();
        _deviceContext.Reset();
        _device.Reset();

        RemoveWindowSubclass(_hwnd, WinAppProc, SUBCLASS_IID); 
    }

    bool EngineCore::VSync() const { return _vSync; }

    void EngineCore::VSync(bool value) { _vSync = value; }

    InputSystem::MouseInputState EngineCore::InputMouseState() const 
    {
       return InputSystem.MouseState;
    }

    InputSystem::KeyboardInputState EngineCore::InputKeyboardState() const 
    {
       return InputSystem.KeyboardState;
    }

    void EngineCore::Quit() 
    {
        _isRun = false;
        Finalize();
    }

    bool EngineCore::SetHWND(uint64_t windowHandle) 
    { 
         _hwnd = reinterpret_cast<HWND>(windowHandle);
        if (SetWindowSubclass(_hwnd, WinAppProc, SUBCLASS_IID, reinterpret_cast<DWORD_PTR>(this)))
        {
            return true; 
        }
        else
        {
            return false;
        }
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

        ImGui::CreateContext();
        ImGuiIO& io = ImGui::GetIO();
        io.ConfigFlags |= ImGuiConfigFlags_NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags_NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags_DockingEnable;     
        ImGui_ImplWin32_Init(_hwnd);
        ImGui_ImplDX11_Init(_device.Get(), _deviceContext.Get());

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

        _rtvBackBufferID = _randerTargetViews.create(ComPtr<ID3D11RenderTargetView>());
        auto& backBufferRTV = _randerTargetViews.at(_rtvBackBufferID);
        hr = _device->CreateRenderTargetView(backBuffer.Get(), nullptr, backBufferRTV.GetAddressOf());
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
