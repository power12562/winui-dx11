using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Diagnostics;
using WsiuEngine.Collections;

namespace WsiuEngine.Core.System
{
    public static partial class ReflectionObject
    {
        public static partial class SerializedOption
        {
            public static JsonSerializerOptions JsonOption
            {
                get => jsonOption.Value;
                set => jsonOption = new(() => new JsonSerializerOptions(value));
            }
            private static Lazy<JsonSerializerOptions> jsonOption = new(() => new JsonSerializerOptions(DefaultJsonOption));

            public static JsonSerializerOptions DefaultJsonOption => defaultJsonOption.Value;
            private static readonly Lazy<JsonSerializerOptions> defaultJsonOption = new(() =>
            new JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = true,
                AllowTrailingCommas = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                Converters = { 
                    new IdProviderJsonConverter()
                }
            });
        }

        public static string SerializeToJson(object obj)
        {
            return SerializeToJson(obj, SerializedOption.JsonOption);
        }

        public static string SerializeToJson(object obj, JsonSerializerOptions options)
        {
            IReadOnlyList<Field> fields = GetFields(obj);
            if (fields.Count == 0)
                return string.Empty;

            if (obj is ISerializationCallback target)
            {
                target.OnBeforeSerialize();
            }

            string json = string.Empty;
            Dictionary<string, object> fieldsNode = [];
            foreach (var field in fields)
            {
                if (field.IsProperty)
                    continue;

                var setter = field.Set;
                if (setter == null)
                    continue;

                var getter = field.Get;
                if (getter == null)
                    continue;

                object? value = getter(obj);
                if (value == null)
                    continue;

                Type type = field.Type;
                if (type.IsClass && IsSystemNamespace(type) == false)
                {
                    if (typeof(IIdentity).IsAssignableFrom(type))
                    {
                        IIdentity identity = (IIdentity)value;
                        value = SerializeIdentityToJson(identity, options);
                    }
                    else if (Member.HasAttribute<SerializableClassAttribute>(field.TypeAttributes))
                    {
                        value = SerializeToJson(value, options);
                    }
                    else
                    {
                        continue;
                    }
                }

                string name = field.Name;
                fieldsNode[name] = value;
            }

            json = JsonSerializer.Serialize(fieldsNode, options);
            return json;
        }

        private static object SerializeIdentityToJson(IIdentity identity, JsonSerializerOptions options)
        {
            if (identity.IsEntity == true) 
            {
                // Entity는 GUID를 참조.
                return identity.UId;
            }
            else
            {
                return SerializeToJson(identity, options);
            }
        }

        internal struct IdEntityRecord
        {
            public object Owner;    
            public Field Field;     
            public Guid Uid;     
        }
        private static readonly ThreadLocal<List<IdEntityRecord>> recordListBuffer = new(() => []);
        private static readonly ThreadLocal<List<ISerializationCallback>> callbackListBuffer = new(() => []);

        public static void DeserializeFromJson(object obj, string json)
        {
            DeserializeFromJson(obj, json, SerializedOption.JsonOption);
        }

        public static void DeserializeFromJson(object obj, string json, JsonSerializerOptions options)
        {
            List<IdEntityRecord> records = recordListBuffer.Value!;
            List<ISerializationCallback> callbacks = callbackListBuffer.Value!;
            records.Clear();
            callbacks.Clear();
            PoulateFromJson(obj, json, ref records, ref callbacks, options);
            ResolveReferences(records);
            foreach (ISerializationCallback callback in callbacks)
            {
                callback.OnAfterDeserialize();
            }
        }

        internal static void PoulateFromJson(object obj, string json, ref List<IdEntityRecord> records, ref List<ISerializationCallback> callbacks, JsonSerializerOptions options)
        {
            records ??= [];
            callbacks ??= [];

            if (string.IsNullOrEmpty(json))
                return;

            IReadOnlyList<Field> fields = GetFields(obj);
            if (fields.Count == 0)
                return;

            Dictionary<string, JsonElement>? jsonElements;
            try
            {
                jsonElements = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, options);
            }
            catch (Exception ex)
            {
                //TODO: 이후 로그 작성 필요
                Debug.WriteLine(ex);
                return;
            }

            if (jsonElements == null)
                return;

            if (jsonElements.Count == 0)
                return;

            foreach (var field in fields)
            {
                if (field.IsProperty)
                    continue;

                var setter = field.Set;
                if (setter == null)
                    continue;

                Type type = field.Type;
                bool isIdEntity = false;
                bool isSerializableClass = false;
                if (type.IsClass && IsSystemNamespace(type) == false)
                {
                    if (typeof(IIdentity).IsAssignableFrom(type))
                        isIdEntity = true;
                    else if (Member.HasAttribute<SerializableClassAttribute>(field.TypeAttributes))
                        isSerializableClass = true;
                    else
                        continue;
                }

                string name = field.Name;
                if (jsonElements.TryGetValue(name, out JsonElement element))
                {
                    try
                    {
                        if (element.ValueKind == JsonValueKind.Null)
                            continue;

                        if (isIdEntity)
                        {
                            Guid? uid = element.Deserialize<Guid>(options);
                            if (uid != null)
                            {
                                records.Add(new IdEntityRecord
                                {
                                    Owner = obj,
                                    Field = field,
                                    Uid = (Guid)uid,
                                });
                            }
                            continue;
                        }

                        if (isSerializableClass == true)
                        {
                            object? fieldObj = field.Get(obj);
                            if (fieldObj != null)
                            {
                                string? rawJson = element.GetString();
                                if(rawJson != null)
                                    PoulateFromJson(fieldObj, rawJson, ref records, ref callbacks, options);
                            }                        
                            continue;
                        }

                        object? value = element.Deserialize(type, options);
                        if (value == null)
                            continue;

                        setter(obj, value);
                    }
                    catch
                    {
                        //TODO: 이후 로그 작성 필요
                    }
                }
            }

            if (obj is ISerializationCallback target)
            {
                callbacks.Add(target);
            }
        }    
        
        internal static void ResolveReferences(List<IdEntityRecord> records)
        {
            if (records.Count == 0)
                return;

            foreach (IdEntityRecord record in records)
            {
                var setter = record.Field.Set;
                if (setter == null)
                    continue;

                //TODO: IdEntity 리소스 가져와서 참조 연결하는 로직 필요.
                setter(record.Owner, null);         
            }

            records.Clear();
        }
    }
}
