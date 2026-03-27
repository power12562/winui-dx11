using System;
using WsiuEditor.Interfaces;

namespace WsiuEditor.System
{
    public partial class EditorManager
    {
        private void DrawMainMenuBar()
        {
            _imguiContext.BeginMainMenuBar();
            DrawWindowMenu();
            DrawLayoutMenu();
            _imguiContext.EndMainMenuBar();
        }

        private void DrawWindowMenu()
        {
            _imguiContext.BeginMenu("Window");
            DrawCreateEditorMenuItems();
            _imguiContext.EndMenu();
        }

        private void DrawLayoutMenu()
        {
            _imguiContext.BeginMenu("Layout");
            _imguiContext.MenuItem("Show in Explorer", ShowLayoutInExplorer);
            _imguiContext.MenuItem("Export Layout", ExportLayoutWithDialog);
            _imguiContext.MenuItem("Import Layout", ImportLayoutWithDialog);       
            _imguiContext.EndMenu();
        }

        private void DrawCreateEditorMenuItems()
        {
            static string GetDisplayName(Type editorType)
            {
                const string suffix = "Editor";
                string displayName = editorType.Name;
                if (displayName.EndsWith(suffix) == true)
                {
                    displayName = displayName[..^suffix.Length];
                }
                return displayName;
            }

            if (0 < EditorManager.transientProvider.Count)
            {
                _imguiContext.SeparatorText("Transient");
                foreach (Type key in EditorManager.transientProvider.Keys)
                {
                    Type type = key;
                    _imguiContext.MenuItem(GetDisplayName(type), () =>
                    {
                        InternalCreateTransientEditor(type);
                    });
                }
            }

            if (0 < EditorManager.singletonProvider.Count)
            {
                _imguiContext.SeparatorText("Singleton");
                foreach (Type key in EditorManager.singletonProvider.Keys)
                {
                    Type type = key;
                    bool isActive = false;
                    if (_singletonEditorTypeToInstance.TryGetValue(type, out IEditor? editor))
                    {
                        isActive = editor.Active;
                    }

                    _imguiContext.MenuItem(GetDisplayName(type), isActive, () =>
                    {
                        if (isActive == false)
                        {
                            InternalActiveSingletonEditor(type);
                        }
                        else
                        {
                            editor!.Active = false;
                        }
                    });
                }
            }
        }
    }
}
