using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WsiuEngine.Core.System
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SerializableClassAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class HideInInspectorAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SerializeFieldAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class SerializeMethodAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ReadOnlyFieldAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MultilineStringFieldAttribute : Attribute 
    {
        public float Width = 0f;
        public float Height = 0f;
    }

    public static partial class ReflectionObject
    {
        public delegate object? MethodInvoker(object target, object[]? args);

        public class Field
        {
            public string Name { get; init; } = null!;
            public Type Type { get; init; } = null!;
            public Func<object, object?> Get { get; init; } = null!;
            public Action<object, object?>? Set { get; init; }

            public IReadOnlyDictionary<Type, Attribute> TypeAttributes { get; init; } = null!;
            public IReadOnlyDictionary<Type, Attribute> FieldAttributes { get; init; } = null!;
        }
        public class Method
        {
            public string Name { get; init; } = null!;
            public string DisplayName = null!;
            public Type ReturnType = null!;
            public MethodInvoker Invoker { get; init; } = null!;
            public List<ParameterInfo> Parameters = null!;
            public IReadOnlyDictionary<Type, Attribute> MethodAttributes { get; init; } = null!;
        }
        public class Member(Type type)
        {
            public IReadOnlyList<Field> Fields => fields;
            private readonly List<Field> fields = CreateSerializeFields(type);

            public IReadOnlyList<Method> Methods => methods;
            private readonly List<Method> methods = CreateSerializeMethods(type);

            public static bool HasAttribute<TAttribute>(IReadOnlyDictionary<Type, Attribute> attributes) where TAttribute : Attribute
            {
                return attributes.ContainsKey(typeof(TAttribute));
            }

            public static TAttribute? GetAttribute<TAttribute>(IReadOnlyDictionary<Type, Attribute> attributes) where TAttribute : Attribute
            {
                attributes.TryGetValue(typeof(TAttribute), out var attribute);
                return (TAttribute?)attribute;
            }

            public static Dictionary<Type, Attribute> GetAttributes(MemberInfo info)
            {
                var attributes = info.GetCustomAttributes(true).Cast<Attribute>();
                Dictionary<Type, Attribute> dictionary = [];
                foreach (var attribute in attributes)
                {
                    Type type = attribute.GetType();
                    dictionary[type] = attribute;
                }
                return dictionary;
            }
        }

        public static bool IsSystemNamespace(Type type)
        {
            return type.Namespace != null && type.Namespace.StartsWith("System");
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

            foreach (FieldInfo field in type.GetFields(flags))
            {
                Dictionary<Type, Attribute> attributes = Member.GetAttributes(field);
                Type fieldType = field.FieldType;
                if (field.IsPublic || attributes.ContainsKey(typeof(SerializeFieldAttribute)))
                {
                    bool isReadOnly = Member.HasAttribute<ReadOnlyFieldAttribute>(attributes);
                    list.Add(new Field
                    {
                        Name = field.Name,
                        Type = fieldType,
                        Get = (obj) => field.GetValue(obj),
                        Set = isReadOnly ? null : (obj, value) => field.SetValue(obj, value),
                        FieldAttributes = attributes,
                        TypeAttributes = Member.GetAttributes(fieldType)
                    });
                }
            }

            foreach (PropertyInfo property in type.GetProperties(flags))
            {
                if (property.GetIndexParameters().Length > 0) continue;

                Dictionary<Type, Attribute> attributes = Member.GetAttributes(property);
                Type propertyType = property.PropertyType;
                bool isPublicRead = property.CanRead && property.GetMethod!.IsPublic;
                bool isPublicWrite = property.CanWrite && property.SetMethod!.IsPublic;
                bool isAttribute = attributes.ContainsKey(typeof(SerializeFieldAttribute));
                bool isNotReadOnly = Member.HasAttribute<ReadOnlyFieldAttribute>(attributes) == false;
                if (isPublicRead || isAttribute)
                {
                    Action<object, object?>? setter;
                    if(isAttribute)
                    {
                        setter = isNotReadOnly && property.CanWrite ? (obj, value) => property.SetValue(obj, value) : null;
                    }
                    else
                    {
                        setter = isNotReadOnly && isPublicWrite ? (obj, value) => property.SetValue(obj, value) : null;
                    }

                    list.Add(new Field
                    {
                        Name = property.Name,
                        Type = propertyType,
                        Get = (obj) => property.GetValue(obj),
                        Set = setter,
                        FieldAttributes = attributes,
                        TypeAttributes = Member.GetAttributes(propertyType)
                    });
                }
            }

            return list;
        }

        private static List<Method> CreateSerializeMethods(Type type)
        {
            var list = new List<Method>();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (MethodInfo method in type.GetMethods(flags))
            {
                var attributes = Member.GetAttributes(method);
                if (Member.HasAttribute<SerializeMethodAttribute>(attributes) == true)
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
                        MethodAttributes = attributes
                    });             
                }
            }
            return list;
        }
    }
}
