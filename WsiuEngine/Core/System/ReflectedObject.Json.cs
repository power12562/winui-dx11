using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WsiuEngine.Core.System
{
    public static partial class ReflectedObject
    {
        public static partial class SerializedOption
        {
            public static JsonSerializerOptions JsonOption
            {
                get => jsonOption.Value;
                set => jsonOption = new(() => new JsonSerializerOptions(value));
            }
            public static Lazy<JsonSerializerOptions> jsonOption = new(() => new JsonSerializerOptions(DefaultJsonOption));

            public static JsonSerializerOptions DefaultJsonOption => defaultJsonOption.Value;
            private static readonly Lazy<JsonSerializerOptions> defaultJsonOption = new(() =>
            new JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            });
        }

        public static string SerializeToJson(object obj)
        {
            IReadOnlyList<Field> fields = GetFields(obj);
            if (fields.Count == 0)
                return string.Empty;

            string json = string.Empty;
            Dictionary<string, object> fieldsNode = [];
            foreach (var field in fields)
            {              
                var setter = field.Set;
                if (setter == null)
                    continue;

                var getter = field.Get;
                if (getter == null)
                    continue;

                Type type = field.Type;
                if (type.IsClass && IsSystemNamespace(type) == false)
                    continue;

                object? value = getter(obj);
                if (value == null)
                    continue;

                string name = field.Name;
                fieldsNode[name] = value;   
            }

            json = JsonSerializer.Serialize(fieldsNode, SerializedOption.JsonOption);
            return json;
        }

        public static void DeserializeFromJson(object obj, string json)
        {
            IReadOnlyList<Field> fields = GetFields(obj);
            if (fields.Count == 0)
                return;

            var jsonElements = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, SerializedOption.JsonOption);
            if (jsonElements == null)
                return;

            if (jsonElements.Count == 0)
                return;

            foreach (var field in fields)
            {
                var setter = field.Set;
                if (setter == null)
                    continue;

                Type type = field.Type;
                if (type.IsClass && IsSystemNamespace(type) == false)
                    continue;

                string name = field.Name;
                if (jsonElements.TryGetValue(name, out JsonElement element))
                {
                    try
                    {
                        if (element.ValueKind == JsonValueKind.Null) continue;

                        object? value = element.Deserialize(type, SerializedOption.JsonOption);
                        if (value != null)
                            setter(obj, value);
                    }
                    catch
                    {}
                }
            }
        }
    }
}
