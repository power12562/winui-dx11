using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace WsiuEngine.Core.System
{
    public class WindowService
    {
        private static WindowService instance = null!;

        internal static void Initialize(Window mainWindow)
        {
            instance = new WindowService(mainWindow);
        }

        private readonly Window _mainWindow;
        private WindowId _windowId;
        private WindowService(Window mainWindow)
        {
            _mainWindow = mainWindow;
            nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);
            _windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        }

        public static string AppLocalFolderPath => lazyAppLocalFolderPath.Value;
        private static readonly Lazy<string> lazyAppLocalFolderPath = new(() => ApplicationData.Current.LocalFolder.Path);

        public static FileOpenPicker CreateFileOpenPicker()
        {
            return instance.InternalCreateFileOpenPicker();
        }
        internal FileOpenPicker InternalCreateFileOpenPicker()
        {
            FileOpenPicker picker = new(_windowId);
            return picker;
        }

        public static FileSavePicker CreateFileSavePicker()
        {
            return instance.InternalCreateFileSavePicker();
        }
        internal FileSavePicker InternalCreateFileSavePicker()
        {
            FileSavePicker picker = new(_windowId);
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

        public static Task<PickFileResult> GetSaveFilePathAsync(string suggestedFileName, params string[] fileTypeChoices)
        {
            return instance.InternalGetSaveFilePathAsync(suggestedFileName, fileTypeChoices);
        }
        internal Task<PickFileResult> InternalGetSaveFilePathAsync(string suggestedFileName, params string[] fileTypeChoices)
        {
            FileSavePicker picker = InternalCreateFileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.SuggestedFileName = suggestedFileName;
            if (fileTypeChoices.Length == 0)
            {
                picker.FileTypeChoices.Add("All Files", ["*"]);
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

        public static Task<ContentDialogResult> ShowContentDialogAsync(string title, string content, string primaryButtonText = "확인", string closeButtonText = "취소")
        {
            return instance.InternalShowContentDialogAsync(title, content, primaryButtonText, closeButtonText);
        }

        internal Task<ContentDialogResult> InternalShowContentDialogAsync(string title, string content, string primaryButtonText, string closeButtonText)
        {
            ContentDialog dialog = new()
            {
                Title = title,
                Content = content,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = closeButtonText,
                XamlRoot = _mainWindow.Content.XamlRoot
            };
            return dialog.ShowAsync().AsTask();
        }
    }
}
