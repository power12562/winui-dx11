using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.PointOfService;

namespace WsiuEngine.Core.System
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SerializeFieldAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class SerializeMethodAttribute : Attribute { }

    public static partial class ReflectedObject
    {
        public delegate object? MethodInvoker(object target, object[]? args);

        public class Field
        {
            public string Name { get; init; } = null!;
            public Type Type { get; init; } = null!;
            public Func<object, object?> Get { get; init; } = null!;
            public Action<object, object?>? Set { get; init; }
        }
        public class Method
        {
            public string Name { get; init; } = null!;
            public string DisplayName = null!;
            public Type ReturnType = null!;
            public MethodInvoker Invoker { get; init; } = null!;
            public List<ParameterInfo> Parameters = null!;
        }
        public class Member(Type type)
        {
            public IReadOnlyList<Field> Fields => fields;
            private readonly List<Field> fields = CreateSerializeFields(type);

            public IReadOnlyList<Method> Methods => methods;
            private readonly List<Method> methods = CreateSerializeMethods(type);
        }

        private static readonly Dictionary<Type, Member> reflectDataBase = [];
        public static IReadOnlyList<Field> GetFields(object obj)
        {
            if (obj == null) return [];

            Type type = obj.GetType();
            Member data = TryInsert(type);
            return data.Fields;
        }
        public static IReadOnlyList<Method> GetMethods(object obj)
        {
            if (obj == null) return [];

            Type type = obj.GetType();
            Member data = TryInsert(type);
            return data.Methods;
        }

        private static Member TryInsert(Type type)
        {
            if (reflectDataBase.TryGetValue(type, out Member? data) == false)
            {
                data = new Member(type);
                reflectDataBase[type] = data;
            }
            return data;
        }

        private static List<Field> CreateSerializeFields(Type type)
        {
            var list = new List<Field>();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (var field in type.GetFields(flags))
            {
                if (field.IsPublic || field.GetCustomAttribute<SerializeFieldAttribute>() != null)
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

            foreach (var property in type.GetProperties(flags))
            {
                if (property.GetIndexParameters().Length > 0) continue;

                bool isPublicRead = property.CanRead && property.GetMethod!.IsPublic;
                bool isPublicWrite = property.CanWrite && property.SetMethod!.IsPublic;
                bool isAttribute = property.GetCustomAttribute<SerializeFieldAttribute>() != null;
                if (isPublicRead || isAttribute)
                {
                    Action<object, object?>? setter;
                    if(isAttribute)
                    {
                        setter = property.CanWrite ? (obj, value) => property.SetValue(obj, value) : null;
                    }
                    else
                    {
                        setter = isPublicWrite ? (obj, value) => property.SetValue(obj, value) : null;
                    }

                    list.Add(new Field
                    {
                        Name = property.Name,
                        Type = property.PropertyType,
                        Get = (obj) => property.GetValue(obj),
                        Set = setter,
                    });
                }
            }

            return list;
        }

        private static List<Method> CreateSerializeMethods(Type type)
        {
            var list = new List<Method>();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (var method in type.GetMethods(flags))
            {
                if (method.GetCustomAttribute<SerializeMethodAttribute>() != null)
                {
                    string name = method.Name;
                    Type returnType = method.ReturnType;
                    object? methodInvoker(object obj, object[]? args) => method.Invoke(obj, args);
                    List<ParameterInfo> parameters = [.. method.GetParameters()];
                    string parametersDisplay = string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));
                    string displayName = $"{returnType.Name} {name}({parametersDisplay})";
                    list.Add(new Method
                    {
                        Name = name,
                        DisplayName = displayName,
                        ReturnType = returnType,
                        Invoker = methodInvoker,
                        Parameters = parameters,
                    });             
                }
            }
            return list;
        }
    }
}
