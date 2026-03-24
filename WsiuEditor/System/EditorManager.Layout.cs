using System;
using System.IO;
using System.Threading.Tasks;
using WsiuRenderer;
using Windows.Storage;
using WsiuEngine.Core.System;

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

        [SerializeField]
        [HideInInspector]
        private string _imguiLayoutSettings;

        public void OnBeforeSerialize()
        {
            _imguiLayoutSettings = ImguiContext.SaveIniSettingsToMemory();
        }
        public void OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(_imguiLayoutSettings) == true)
                return;

            ImguiContext.LoadIniSettingsFromMemory(_imguiLayoutSettings);
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
