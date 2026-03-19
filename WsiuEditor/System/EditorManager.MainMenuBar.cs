using ABI.WsiuRenderer;
using System;
using System.Collections.Generic;
using WsiuEditor.Editor;
using WsiuEngine.Core;
using WsiuRenderer;

namespace WsiuEditor.System
{
    internal partial class EditorManager
    {
        private void DrawMainMenuBar()
        {
            _imguiContext.BeginMainMenuBar();
            DrawWindowMenu();
            _imguiContext.EndMainMenuBar();
        }

        private void DrawWindowMenu()
        {
            _imguiContext.BeginMenu("Window");
            DrawCreateEditorMenuItems();
            _imguiContext.EndMenu();
        }

        private void DrawCreateEditorMenuItems()
        {
            foreach (var editorType in editorProvider)
            {
                Type type = editorType.Key;
                ActionCreateEditor createEditor = editorType.Value;
                _imguiContext.MenuItem(type.Name, () =>
                {
                    IEditor editor = createEditor(_engine, AddEditorId(type));
                    editor.SetDisableCallback(CleanUpEditors);
                    _editors.Add(editor);                
                });
            }
        }
    }
}
