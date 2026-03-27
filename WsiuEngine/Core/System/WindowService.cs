using Microsoft.UI;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
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

        public static Task<PickFileResult> GetOpenFilePathAsync(params string[] fileTypeFilter)
        {
            return instance.InternalGetOpenFilePathAsync(fileTypeFilter);
        }
        internal Task<PickFileResult> InternalGetOpenFilePathAsync(params string[] fileTypeFilter)
        {
            FileOpenPicker picker = InternalCreateFileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            if (fileTypeFilter.Length == 0)
            {
                picker.FileTypeFilter.Add("*");
            }
            else
            {
                foreach (string filter in fileTypeFilter)
                {
                    picker.FileTypeFilter.Add(filter);
                }
            }
            return picker.PickSingleFileAsync().AsTask();     
        }   

        public static Task<PickFileResult> GetSaveFilePathAsync(params string[] fileTypeChoices)
        {
            return instance.InternalGetSaveFilePathAsync(fileTypeChoices);
        }
        internal Task<PickFileResult> InternalGetSaveFilePathAsync(params string[] fileTypeChoices)
        {
            FileSavePicker picker = InternalCreateFileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            if (fileTypeChoices.Length == 0)
            {
                picker.FileTypeChoices.Add("any", ["*"]);
            }
            else if (fileTypeChoices.Length == 1)
            {
                picker.FileTypeChoices.Add(fileTypeChoices[0], ["*"]);
            }
            else
            {
                picker.FileTypeChoices.Add(fileTypeChoices[0], fileTypeChoices.Skip(1).ToList());
            }
            return picker.PickSaveFileAsync().AsTask(); 
        }
    }
}
