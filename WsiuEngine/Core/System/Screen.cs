using Microsoft.UI;
using Microsoft.UI.Windowing;
using System.Runtime.InteropServices;

namespace WsiuEngine.Core.System
{
    public partial class Screen
    {
        private readonly nint _hwnd;
        private readonly WindowId _windowID;
        private readonly AppWindow _appWindow;
        public Screen(nint hwnd)
        {
            _hwnd = hwnd;
            _windowID = Win32Interop.GetWindowIdFromWindow(hwnd);
            _appWindow = AppWindow.GetFromWindowId(_windowID);
        }

        public struct Bounds(int x, int y, int width, int height)
        {
            public int X = x;
            public int Y = y;
            public int Width = width;
            public int Height = height;
        }

        public int Width => _appWindow.Size.Width;
        public int Height => _appWindow.Size.Height;

        public int PosX => _appWindow.Position.X;
        public int PosY => _appWindow.Position.Y;

        public bool IsMaximized
        {
            get => _appWindow.Presenter is OverlappedPresenter { State: OverlappedPresenterState.Maximized };
        }

        public bool IsFullScreen
        {
            get => _appWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen;
        }

        public void Move(int x, int y)
        {
            _appWindow.Move(new(x, y));
        }

        public void Resize(int width, int height)
        {
            _appWindow.ResizeClient(new(width, height));
        }

        public void Maximize()
        {
            if (_appWindow.Presenter is OverlappedPresenter overlapped)
                overlapped.Maximize();
        }

        public Bounds RestoreBounds
        { 
            get
            {
                WINDOWPLACEMENT wp = new()
                {
                    length = Marshal.SizeOf<WINDOWPLACEMENT>()
                };
                if (GetWindowPlacement(_hwnd, ref wp))
                {
                    int x = wp.rcNormalPosition.left;
                    int y = wp.rcNormalPosition.top;
                    int width = wp.rcNormalPosition.right - wp.rcNormalPosition.left;
                    int height = wp.rcNormalPosition.bottom - wp.rcNormalPosition.top;
                    return new(x, y, width, height);
                }
                return new(PosX, PosY, Width, Height);
            }
        }
    }
}
