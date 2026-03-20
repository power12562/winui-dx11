using System;
using System.IO;
using System.Threading.Tasks;
using WsiuRenderer;
using Windows.Storage;

namespace WsiuEditor.System
{
    public partial class EditorManager
    {
        public static string ApplicationLocalFolderPath => lazyApplicationLocalFolderPath.Value;
        private static readonly Lazy<string> lazyApplicationLocalFolderPath = new(() => ApplicationData.Current.LocalFolder.Path);
        private const string imguiLayoutFilename = "editorImguiLayout.ini";
        private static string GetDefaultLayoutPath()
        {
            return Path.Combine(EditorManager.ApplicationLocalFolderPath, EditorManager.imguiLayoutFilename);
        }

        public static async Task SaveImguiLayoutAsync()
        {      
            await SaveImguiLayoutAsync(GetDefaultLayoutPath());
        }
        public static async Task SaveImguiLayoutAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) 
                throw new ArgumentNullException(nameof(filePath));

            string? directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            string settings = ImguiContext.SaveIniSettingsToMemory();
            await File.WriteAllTextAsync(filePath, settings);
        }

        public static async Task LoadImguiLayoutAsync()
        {
            await LoadImguiLayoutAsync(GetDefaultLayoutPath()); 
        }

        public static async Task LoadImguiLayoutAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (Path.Exists(filePath) == false)
            {
                return;
            }
       
            Task<string> task = File.ReadAllTextAsync(filePath);
            string settings = await task;
            ImguiContext.LoadIniSettingsFromMemory(settings);
        }
    }
}
