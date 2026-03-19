using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WsiuEditor.Editor;
using WsiuEngine.Core;

namespace WsiuEditor.System
{
    internal partial class EditorManager
    {
        public delegate IEditor ActionCreateEditor(Engine engine, UInt64 id);

        private static readonly Dictionary<Type, ActionCreateEditor> editorProvider = [];
        public static void RegisterEditors()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IEditor).IsAssignableFrom(p)
                         && !p.IsInterface
                         && !p.IsAbstract);

            foreach (var type in types)
            {
                editorProvider[type] = (engine, id) =>
                {
                    object? instance = Activator.CreateInstance(type, engine, id);
                    if (instance is IEditor editor)
                    {
                        return editor;
                    }
                    throw new InvalidOperationException($"{type.Name}은 IEditor를 구현하지 않았습니다.");
                };
            }
        }
    }
}
