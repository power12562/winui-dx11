using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

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

            if (obj is ISerializationCallback target)
            {
                target.OnBeforeSerialize();
            }

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

                object? value = getter(obj);
                if (value == null)
                    continue;

                Type type = field.Type;
                if (type.IsClass && IsSystemNamespace(type) == false)
                {
                    if (typeof(IIdentity).IsAssignableFrom(type))
                    {
                        IIdentity identity = (IIdentity)value;
                        value = SerializeIdentityToJson(identity);
                    }
                    else if (Member.HasAttribute<SerializableClassAttribute>(field.TypeAttributes))
                    {
                        value = SerializeToJson(value);
                    }
                    else
                    {
                        continue;
                    }
                }

                string name = field.Name;
                fieldsNode[name] = value;
            }

            json = JsonSerializer.Serialize(fieldsNode, SerializedOption.JsonOption);
            return json;
        }

        private static object SerializeIdentityToJson(IIdentity identity)
        {
            if (identity.IsEntity == true) 
            {
                // Entity는 GUID를 참조.
                return identity.UId;
            }
            else
            {
                return SerializeToJson(identity);
            }
        }

        internal struct IdEntityRecord
        {
            public object Owner;    
            public Field Field;     
            public Guid Uid;     
        }
        private static readonly ThreadLocal<List<IdEntityRecord>> recordList = new(() => []);
        public static void DeserializeFromJson(object obj, string json)
        {
            List<IdEntityRecord> records = recordList.Value!;
            records.Clear();
            PoulateFromJson(obj, json, ref records);
            ResolveReferences(records);
            if (obj is ISerializationCallback target)
            {
                target.OnAfterDeserialize();
            }
        }

        internal static void PoulateFromJson(object obj, string json, ref List<IdEntityRecord> records)
        {
            records ??= [];

            if (string.IsNullOrEmpty(json))
                return;

            IReadOnlyList<Field> fields = GetFields(obj);
            if (fields.Count == 0)
                return;

            Dictionary<string, JsonElement>? jsonElements;
            try
            {
                jsonElements = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, SerializedOption.JsonOption);
            }
            catch
            {
                //TODO: 이후 로그 작성 필요
                return;
            }

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
                            object? uid = element.Deserialize(typeof(Guid), SerializedOption.JsonOption);
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
                                PoulateFromJson(fieldObj, element.GetRawText(), ref records);
                            }                        
                            continue;
                        }

                        object? value = element.Deserialize(type, SerializedOption.JsonOption);
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
