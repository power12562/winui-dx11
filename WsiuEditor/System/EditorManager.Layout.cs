using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using WsiuEngine.Collections;
using WsiuEngine.Core;
using WsiuEngine.Core.System;
using WsiuEngine.Extensions;
using WsiuRenderer;

namespace WsiuEditor.System
{
    public partial class EditorManager
    {
        public static string ApplicationLocalFolderPath => lazyApplicationLocalFolderPath.Value;
        private static readonly Lazy<string> lazyApplicationLocalFolderPath = new(() => ApplicationData.Current.LocalFolder.Path);
        private const string editorLayoutFilename = "EditorManagerLayout.ini";
        private static string GetDefaultLayoutPath()
        {
            return Path.Combine(EditorManager.ApplicationLocalFolderPath, EditorManager.editorLayoutFilename);
        }

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

        public void OnBeforeSerialize()
        {
            BeforeSerializeImguiLayout();
            BeforeSerializeTransientEditorLayout();
            BeforeSerializeSingletoneEditorLayout();
        }

        private void BeforeSerializeImguiLayout()
        {
            _layoutSettings.ImguiLayoutSettings = ImguiContext.SaveIniSettingsToMemory();
            Screen screen = Engine.Screen;
            _layoutSettings.IsMaximized = screen.IsMaximized;
            Screen.Bounds bounds = screen.RestoreBounds;
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
            _singletonEditorInstanceLayout = _singletonEditorInstance.Keys.ToList();
        }
        
        public void OnAfterDeserialize()
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

            Screen screen = Engine.Screen;
            screen.Move(_layoutSettings.ScreenPosY, _layoutSettings.ScreenPosY);
            screen.Resize(_layoutSettings.ScreenWidth, _layoutSettings.ScreenHeight);
            if (_layoutSettings.IsMaximized)
                screen.Maximize();
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

            foreach ( var pair in _editorIdProvider)
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

            foreach ( Type type in _singletonEditorInstanceLayout)
            {
                ActiveSingletonEditor(type);
            }
            _singletonEditorInstanceLayout = null;
        }

        public static void SaveLayoutToFile()
        {
            instance.SaveLayoutToFileInternal();
        }

        internal void SaveLayoutToFileInternal()
        {
            SaveLayoutToFileInternal(EditorManager.GetDefaultLayoutPath());
        }

        internal void SaveLayoutToFileInternal(string filePath)
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
            instance.LoadLayoutFromFileInternal();
        }

        internal void LoadLayoutFromFileInternal()
        {
            LoadLayoutFromFileInternal(EditorManager.GetDefaultLayoutPath());
        }

        internal void LoadLayoutFromFileInternal(string filePath)
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

        public async Task SaveLayoutToFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) 
                throw new ArgumentNullException(nameof(filePath));

            string? directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            string settings = ReflectionObject.SerializeToJson(this);
            await File.WriteAllTextAsync(filePath, settings);
        }

        public async Task LoadLayoutFromFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (Path.Exists(filePath) == false)
            {
                return;
            }
       
            Task<string> task = File.ReadAllTextAsync(filePath);
            string settings = await task;
            ReflectionObject.DeserializeFromJson(this, settings);
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
                await Launcher.LaunchFolderAsync(folder);
            }
        }
    }
}
