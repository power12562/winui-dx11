using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using WsiuEditor.Interfaces;
using WsiuEngine.Collections;
using WsiuEngine.Core.System;
using WsiuEngine.Extensions;
using WsiuRenderer;

namespace WsiuEditor.System
{
    public partial class EditorManager
    {
        public static string ApplicationLocalFolderPath => lazyApplicationLocalFolderPath.Value;
        private static readonly Lazy<string> lazyApplicationLocalFolderPath = new(() => ApplicationData.Current.LocalFolder.Path);
        private const string editorLayoutFilename = "EditorManagerLayout.json";
        private static string GetDefaultLayoutPath()
        {
            return Path.Combine(EditorManager.ApplicationLocalFolderPath, EditorManager.editorLayoutFilename);
        }
        public static bool IsLayoutSavedOnClose => instance._isLayoutSavedOnClose;
        private bool _isLayoutSavedOnClose = true;

        struct LayoutSettings
        {
            public string ImguiLayoutSettings;
            public bool IsMaximized;
            public int ScreenPosX;
            public int ScreenPosY;
            public int ScreenWidth;
            public int ScreenHeight;
        }

        [HideInInspector]
        [SerializeField]
        private LayoutSettings _layoutSettings = new()
        {
            ImguiLayoutSettings = "",
            IsMaximized = false,
            ScreenPosX = 0,
            ScreenPosY = 0,
            ScreenWidth = 1920,
            ScreenHeight = 1080
        };

        [HideInInspector]
        [SerializeField]
        private Dictionary<string, IdProvider>? _editorIdProviderLayout = null;

        [HideInInspector]
        [SerializeField]
        private List<Type>? _singletonEditorInstanceLayout = null;

        void ReflectionObject.ISerializationCallback.OnBeforeSerialize()
        {
            BeforeSerializeImguiLayout();
            BeforeSerializeTransientEditorLayout();
            BeforeSerializeSingletoneEditorLayout();
        }

        private void BeforeSerializeImguiLayout()
        {
            _layoutSettings.ImguiLayoutSettings = ImguiContext.SaveIniSettingsToMemory();
            _layoutSettings.IsMaximized = Screen.IsMaximized;
            Screen.Bounds bounds = Screen.RestoreBounds;
            _layoutSettings.ScreenPosX = bounds.X;
            _layoutSettings.ScreenPosY = bounds.Y;
            _layoutSettings.ScreenWidth = bounds.Width;
            _layoutSettings.ScreenHeight = bounds.Height;
        }

        private void BeforeSerializeTransientEditorLayout()
        {
            _editorIdProviderLayout = _editorIdProvider.ToDictionary(
            (pair) => TypeJsonConverter.ConvertTypeToString(pair.Key),
            (pair) => pair.Value
            );
        }

        private void BeforeSerializeSingletoneEditorLayout()
        {
            _singletonEditorInstanceLayout = [];
            foreach (IEditor editor in _singletonEditors)
            {
                if (editor.Active)
                {
                    _singletonEditorInstanceLayout.Add(editor.GetType());
                }
            }
        }

        void ReflectionObject.ISerializationCallback.OnAfterDeserialize()
        {
            AfterDeserializeImguiLayout();
            AfterDeserializeTransientEditorLayout();
            AfterDeserializeSingletonEditorLayout();
        }

        private void AfterDeserializeImguiLayout()
        {
            if (string.IsNullOrEmpty(_layoutSettings.ImguiLayoutSettings) == true)
                return;

            ImguiContext.LoadIniSettingsFromMemory(_layoutSettings.ImguiLayoutSettings);

            Screen.Move(_layoutSettings.ScreenPosY, _layoutSettings.ScreenPosY);
            Screen.Resize(_layoutSettings.ScreenWidth, _layoutSettings.ScreenHeight);
            if (_layoutSettings.IsMaximized)
                Screen.Maximize();
        }

        private void AfterDeserializeTransientEditorLayout()
        {
            if (_editorIdProviderLayout == null)
                return;

            var select = _editorIdProviderLayout.Select(pair => (Key: TypeJsonConverter.ConvertStringToType(pair.Key), pair.Value));
            var where = select.Where(pair => pair.Key != null);
            _editorIdProvider = where.ToDictionary(
                (pair) => pair.Key!,
                (pair) => pair.Value
            );
            _editorIdProviderLayout = null;

            foreach (var pair in _editorIdProvider)
            {
                Type type = pair.Key;
                IdProvider provider = pair.Value;
                foreach (UInt64 id in provider.ActiveIds)
                {
                    CreateTransientEditorWithId(type, id);
                }
            }
        }

        private void AfterDeserializeSingletonEditorLayout()
        {
            if (_singletonEditorInstanceLayout == null)
                return;

            foreach (Type type in _singletonEditorInstanceLayout)
            {
                InternalActiveSingletonEditor(type);
            }
            _singletonEditorInstanceLayout = null;
        }

        public static void SaveLayoutToFile()
        {
            instance.InternalSaveLayoutToFile();
        }

        internal void InternalSaveLayoutToFile()
        {
            InternalSaveLayoutToFile(EditorManager.GetDefaultLayoutPath());
        }

        public static void SaveLayoutToFile(string filePath)
        {
            instance.InternalSaveLayoutToFile(filePath);
        }

        internal void InternalSaveLayoutToFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            string? directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            string settings = ReflectionObject.SerializeToJson(this);
            File.WriteAllText(filePath, settings);
        }

        public static void LoadLayoutFromFile()
        {
            instance.InternalLoadLayoutFromFile();
        }

        internal void InternalLoadLayoutFromFile()
        {
            InternalLoadLayoutFromFile(EditorManager.GetDefaultLayoutPath());
        }

        internal void InternalLoadLayoutFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (Path.Exists(filePath) == false)
            {
                return;
            }

            string settings = File.ReadAllText(filePath);
            LoadLayoutFromMemory(settings);
        }

        private void LoadLayoutFromMemory(string layoutSettings)
        {
            ReflectionObject.DeserializeFromJson(this, layoutSettings);
        }

        public static Task SaveLayoutToFileAsync(string filePath)
        {
            return instance.InternalSaveLayoutToFileAsync(filePath);
        }

        internal Task InternalSaveLayoutToFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            string? directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            string settings = ReflectionObject.SerializeToJson(this);
            return File.WriteAllTextAsync(filePath, settings);
        }

        public static void ShowLayoutInExplorer()
        {
            ShowLayoutInExplorerAsync().Forget();
        }

        public static async Task ShowLayoutInExplorerAsync()
        {
            string path = ApplicationLocalFolderPath;
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
            if (folder != null)
            {
                await Launcher.LaunchFolderAsync(folder).AsTask();
            }
        }

        private void ExportLayoutWithDialog()
        {
            WindowService.GetSaveFilePathAsync("WsiuEditorLayout.json", "WsiuEditor Layout File", ".json").SubmitToEngine((result) =>
            {
                if (result == null)
                    return;

                string savePath = result.Path;
                if (string.IsNullOrEmpty(savePath))
                    return;

                string settings = ReflectionObject.SerializeToJson(this);
                File.WriteAllTextAsync(savePath, settings).Forget();
            });
        }

        private void ImportLayoutWithDialog()
        {
            WindowService.GetOpenFilePathAsync(".json").SubmitToEngine((result) =>
            {
                if (result == null)
                    return;

                string savePath = result.Path;
                if (string.IsNullOrEmpty(savePath))
                    return;

                File.ReadAllTextAsync(savePath).SubmitToEngine((settings) =>
                {
                    _isLayoutSavedOnClose = false;
                    File.WriteAllTextAsync(GetDefaultLayoutPath(), settings).Forget();
                    WindowService.ShowContentDialogAsync(
                    "레이아웃 로드 완료",
                    "새 레이아웃은 재시작 시 적용됩니다. 지금 종료하시겠습니까?",
                    "종료", "나중에").SubmitToEngine((result) =>
                    {
                        switch (result)
                        {
                            case Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary:
                                Application.Quit();
                                break;
                            default:
                                break;
                        }
                    });
                });
            });
        }

    }
}
