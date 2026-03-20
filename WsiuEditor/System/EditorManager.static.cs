using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WsiuEditor.Editor;
using WsiuEngine.Core;

namespace WsiuEditor.System
{
    public partial class EditorManager
    {
        public delegate IEditor TransientProvider(Engine engine, UInt64 id);
        public delegate IEditor SingletonProvider(Engine engine);

        private static readonly Dictionary<Type, TransientProvider> transientProvider = [];
        private static readonly Dictionary<Type, SingletonProvider> singletonProvider = [];
        public static void RegisterEditors()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IEditor).IsAssignableFrom(p)
                         && p.IsSpecialName == false
                         && p.IsClass == true
                         && p.IsAbstract == false);

            foreach (var type in types)
            {
                if (type.GetCustomAttribute<SingletonEditorAttribute>() == null)
                {
                    transientProvider[type] = (engine, id) =>
                    {
                        object? instance = Activator.CreateInstance(type, engine, id);
                        if (instance is IEditor editor)
                        {
                            return editor;
                        }
                        throw new InvalidOperationException($"{type.Name}은 IEditor를 구현하지 않았습니다.");
                    };
                }
                else
                {
                    singletonProvider[type] = (engine) =>
                    {
                        object? instance = Activator.CreateInstance(type, engine, (UInt64)0);
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
}
