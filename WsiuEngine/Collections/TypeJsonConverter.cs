using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WsiuEngine.Collections
{
    public class TypeJsonConverter : JsonConverter<Type>
    {
        private static readonly Dictionary<string, Type> _typeCache = [];

        public static string ConvertTypeToString(Type type)
        {
            string? fullName = type.FullName;
            if (string.IsNullOrEmpty(fullName))
                return string.Empty;

            string? assemblyName = type.Assembly.GetName().Name;
            if (string.IsNullOrEmpty(assemblyName))
                return string.Empty;

            return $"{fullName}, {assemblyName}";
        }

        public static Type? ConvertStringToType(string typeName)
        {
            if (_typeCache.TryGetValue(typeName, out Type? type) == false)
            {
                type = Type.GetType(typeName);
                if (type != null)
                    _typeCache.Add(typeName, type);
            }
            return type;
        }

        public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? typeName = reader.GetString();
            if (string.IsNullOrEmpty(typeName))
                return null;

            return ConvertStringToType(typeName);
        }

        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
        {
            string type = ConvertTypeToString(value);
            writer.WriteStringValue(type);
        }
    }
}
