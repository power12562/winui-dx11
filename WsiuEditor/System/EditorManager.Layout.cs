using System;
using System.IO;
using System.Threading.Tasks;
using WsiuRenderer;
using Windows.Storage;
using WsiuEngine.Core.System;
using WsiuEngine.Core;

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

        public void OnBeforeSerialize()
        {
            _layoutSettings.ImguiLayoutSettings = ImguiContext.SaveIniSettingsToMemory();
            Screen screen = Engine.Screen;
            _layoutSettings.IsMaximized  = screen.IsMaximized;
            Screen.Bounds bounds = screen.RestoreBounds;
            _layoutSettings.ScreenPosX   = bounds.X;
            _layoutSettings.ScreenPosY   = bounds.Y;
            _layoutSettings.ScreenWidth  = bounds.Width;
            _layoutSettings.ScreenHeight = bounds.Height;
        }

        public void OnAfterDeserialize()
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

        public void SaveLayoutToFile()
        {
            SaveLayoutToFile(EditorManager.GetDefaultLayoutPath());
        }

        public void SaveLayoutToFile(string filePath)
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

        public void LoadLayoutFromFile()
        {
            LoadLayoutFromFile(EditorManager.GetDefaultLayoutPath());
        }

        public void LoadLayoutFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (Path.Exists(filePath) == false)
            {
                return;
            }

            string settings = File.ReadAllText(filePath);
            ReflectionObject.DeserializeFromJson(this, settings);
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
    }
}
