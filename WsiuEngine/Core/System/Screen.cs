using Microsoft.UI.Windowing;

namespace WsiuEngine.Core.System
{
    public class Screen
    {
        private readonly AppWindow _appWindow;
        public Screen(AppWindow appWindow)
        {
            _appWindow = appWindow;
        }

        public int Width { get => _appWindow.Size.Width; }
        public int Height { get => _appWindow.Size.Height; }

        public bool IsMaximized
        {
            get => _appWindow.Presenter is OverlappedPresenter { State: OverlappedPresenterState.Maximized };
        }

        public bool IsFullScreen
        {
            get => _appWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen;
        }
    }
}
