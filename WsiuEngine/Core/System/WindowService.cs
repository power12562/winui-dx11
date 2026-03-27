using Microsoft.UI;
using Microsoft.Windows.Storage.Pickers;
using WinRT.Interop;

namespace WsiuEngine.Core.System
{
    public class WindowService
    {
        private static WindowService instance = null!;

        internal static void Initialize(nint hwnd)
        {
            instance = new WindowService(hwnd);
        }

        private nint _hwnd;
        private WindowId _windowId;
        private WindowService(nint hwnd)
        {
            _hwnd = hwnd;
            _windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        }

        public static FileOpenPicker CreateFileOpenPicker()
        {
            return instance.InternalCreateFileOpenPicker();
        }

        internal FileOpenPicker InternalCreateFileOpenPicker()
        {
            FileOpenPicker picker = new(_windowId);
            InitializeWithWindow.Initialize(picker, _hwnd);
            return picker;
        }

        public static FileSavePicker CreateFileSavePicker()
        {
            return instance.InternalCreateFileSavePicker();
        }

        internal FileSavePicker InternalCreateFileSavePicker()
        {
            FileSavePicker picker = new(_windowId);
            InitializeWithWindow.Initialize(picker, _hwnd);
            return picker;
        }   
    }
}
