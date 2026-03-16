using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WsiuEngine.Core.System
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SerializeFieldAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class SerializeMethodAttribute : Attribute { }

    public static partial class ReflectedType<T>
    {
        public class Field
        {
            public string Name { get; init; } = null!;
            public Type Type { get; init; } = null!;
            public Func<T, object?> Get { get; init; } = null!;
            public Action<T, object?>? Set { get; init; }
        }

        public class Method
        {
            public string Name { get; init; } = null!;
            public Action<T, object?[]> Invoker { get; init; } = null!;
        }

        public static IReadOnlyList<Field> Fields => fields.Value;
        private static readonly Lazy<List<Field>> fields = new(() =>
        {
            var list = new List<Field>();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            
            foreach (var field in typeof(T).GetFields(flags))
            {
                if(field.GetCustomAttribute<SerializeFieldAttribute>() != null)
                {
                    list.Add(new Field
                    {
                        Name = field.Name,
                        Type = field.FieldType,
                        Get = (obj) => field.GetValue(obj),
                        Set = (obj, value) => field.SetValue(obj, value),
                    });
                }
            }

            foreach (var property in typeof(T).GetProperties(flags))
            {
                if(property.GetCustomAttribute<SerializeFieldAttribute>() != null)
                {
                    list.Add(new Field
                    {
                        Name = property.Name,
                        Type = property.PropertyType,
                        Get = (obj) => property.GetValue(obj),
                        Set = property.CanWrite ? (obj, value) => property.SetValue(obj, value) : null
                    });
                }
            }
             
            return list;
        });

        public static IReadOnlyList<Method> Methods => methods.Value;
        private static readonly Lazy<List<Method>> methods = new(() =>
        {
            var list = new List<Method>();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (var method in typeof(T).GetMethods(flags))
            {
                if(method.GetCustomAttribute<SerializeMethodAttribute>() != null)
                {
                    list.Add(new Method
                    {
                        Name = method.Name,
                        Invoker = (obj, args) => method.Invoke(obj, args)
                    });
                }
            }
            return list;
        });
    }
}
