#pragma once
#include "EngineCore.g.h"
#include "slot_pool.h"
#include "InputSystem.h"
#include "EditorWindow.h"

namespace winrt::WsiuRenderer::implementation
{
    struct EngineCore : EngineCoreT<EngineCore>
    {
    public:
        using rtv_pool_t = slot_pool<ComPtr<ID3D11RenderTargetView>, ComPtrCleaner<ID3D11RenderTargetView>>;
        using SwapChainPanel = winrt::Microsoft::UI::Xaml::Controls::SwapChainPanel;
        using EditorWindows_t = slot_pool<std::unique_ptr<EditorWindow>, UniquePtrCleaner<EditorWindow>>;

        //idl funtions begin
        EngineCore() = default;
        void Initialize(uint64_t windowHandle, SwapChainPanel const& panel);
        void BeginFrame();
        void Tick();
        void EndFrame();
        void Quit();
        bool VSync() const;
        void VSync(bool value);
        uint64_t RtvBackBufferID() const { return _rtvBackBufferID; }

        InputSystem::MouseInputState    InputMouseState() const;
        InputSystem::KeyboardInputState InputKeyboardState() const;
       
        uint64_t EditorWindowCreate(const hstring& title);
        uint64_t EditorWindowClosableCreate(hstring const& title);
        bool     EditorWindowGetActive(uint64_t id) const;
        void     EditorWindowSetActive(uint64_t id, bool active);
        void     EditorWindowDestroy(uint64_t id);
        void     EditorWindowChangeTitle(uint64_t id, hstring const& newTitle);
        void     EditorWindowBeginCallback(uint64_t id, winrt::WsiuRenderer::EditorWindowCallback const& handler);
        void     EditorWindowDrawCallback(uint64_t id, winrt::WsiuRenderer::EditorWindowCallback const& handler);
        void     EditorWindowEndCallback(uint64_t id, winrt::WsiuRenderer::EditorWindowCallback const& handler);
    public:
        InputSystem InputSystem;

    private:
        static constexpr UINT_PTR SUBCLASS_IID = 1;
        bool SetHWND(uint64_t windowHandle);
        bool CreateDevice();
        bool CreateSwapChain(SwapChainPanel const& panel);
        void Finalize();

        const ComPtr<ID3D11RenderTargetView>& GetBackBufferRTV() const { return _randerTargetViews.at(_rtvBackBufferID); }
        ComPtr<ID3D11RenderTargetView>& GetBackBufferRTV() { return _randerTargetViews.at(_rtvBackBufferID); }

        void Clear();
        void BeginImgui();
        void Update();
        void Render();
        void EndImgui();
        void Flip();

        HWND                           _hwnd            = NULL;
        bool                           _isRun           = true;
        bool                           _vSync           = true;
        ComPtr<ID3D11Device5>          _device;
        ComPtr<ID3D11DeviceContext4>   _deviceContext;
        ComPtr<IDXGISwapChain1>        _swapChain;

        size_t     _rtvBackBufferID = (std::numeric_limits<size_t>::max)();
        rtv_pool_t _randerTargetViews;    

        EditorWindows_t _editorWindows;
    };

} // namespace winrt::WsiuRenderer::implementation

namespace winrt::WsiuRenderer::factory_implementation
{
    struct EngineCore : EngineCoreT<EngineCore, implementation::EngineCore>
    {
    };
} // namespace winrt::WsiuRenderer::factory_implementation
