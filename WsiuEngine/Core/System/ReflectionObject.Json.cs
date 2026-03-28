using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using WsiuEngine.Collections;
using WsiuEngine.Core.Interfaces;

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
                    new IdProviderJsonConverter(),
                    new TypeJsonConverter(),
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
                value = SerializeElementToJson(type, value, options);

                if(value == null)
                    continue;

                string name = field.Name;
                fieldsNode[name] = value;
            }

            json = JsonSerializer.Serialize(fieldsNode, options);
            return json;
        }

        internal static string SerializeElementToJson(Type type, object value, JsonSerializerOptions options)
        {
            bool isSystemNamespaceType = IsSystemNamespace(type);
            if (value is not string && value is IEnumerable enumerable)
            {
                Type[]? elementTypes = null;
                if (type.IsArray)
                {
                    Type? elementType = type.GetElementType();
                    if (elementType == null)
                        return string.Empty;

                    elementTypes = [elementType];
                }
                else if (type.IsGenericType)
                {
                    elementTypes = type.GetGenericArguments();
                }

                if (elementTypes == null || elementTypes.Length == 0)
                    return string.Empty;

                bool isSerializable = true;
                foreach (Type elementType in elementTypes)
                {
                    if (isSystemNamespaceType == false && Member.HasAttribute<SerializableClassAttribute>(GetTypeAttributes(elementType)) == false)
                    {
                        isSerializable = false;
                        break;
                    }
                }

                if (isSerializable == false)
                    return string.Empty;

                if (enumerable is IDictionary dictionary)
                {
                    return SerializeDictionaryToJson(dictionary, options);
                }
                else
                {
                    return SerializeEnumerableToJson(enumerable, options);
                }
            }
            else if (type.IsClass && isSystemNamespaceType == false)
            {
                if (value is IIdentity identity)
                {
                    return SerializeIdentityToJson(identity, options);
                }
                else if (Member.HasAttribute<SerializableClassAttribute>(GetTypeAttributes(type)))
                {
                    return SerializeToJson(value, options);
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return JsonSerializer.Serialize(value, options);
            }
        }
        internal static string SerializeDictionaryToJson(IDictionary dictionary, JsonSerializerOptions options)
        {
            List<KeyValuePair<string, string>> serializeTemp = [];
            foreach (DictionaryEntry entry in dictionary)
            {
                object key = entry.Key;
                object? value = entry.Value;
                if (value == null)
                    continue;

                string jsonKey = SerializeElementToJson(key.GetType(), key, options);
                string jsonValue = SerializeElementToJson(value.GetType(), value, options);
                if (string.IsNullOrEmpty(jsonKey))
                    continue;
                if (string.IsNullOrEmpty(jsonValue))
                    continue;

                serializeTemp.Add(new(jsonKey, jsonValue));
            }
            string json = JsonSerializer.Serialize(serializeTemp, options);
            return json;
        }

        internal static string SerializeEnumerableToJson(IEnumerable enumerable, JsonSerializerOptions options)
        {
            List<string> serializeTemp = [];
            foreach (object? item in enumerable)
            {
                if (item == null)
                    continue;

                string json = SerializeElementToJson(item.GetType(), item, options);
                if (string.IsNullOrEmpty(json))
                    continue;

                serializeTemp.Add(json);
            }
            return JsonSerializer.Serialize(serializeTemp, options);
        }

        private static string SerializeIdentityToJson(IIdentity identity, JsonSerializerOptions options)
        {
            if (identity.IsEntity == true)
            {
                // Entity는 GUID를 참조.
                return JsonSerializer.Serialize(identity.UId, options);
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
                Debug.WriteLine($"[Deserialize Error] {ex.Message}");
                if (Debugger.IsAttached)
                    Debugger.Break();
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
                bool isSystemNamespaceType = IsSystemNamespace(type);
                bool isDictionary = false;
                bool isEnumerable = false;
                object? fieldValue = field.Get(obj);
                bool IsAssignableFrom<TType>(Type assignableType)
                {
                    if (fieldValue != null)
                        return fieldValue is TType;
                    else
                        return assignableType.IsAssignableFrom(type);
                }
                if (type != Types.String && IsAssignableFrom<IEnumerable>(Types.IEnumerable))
                {
                    if (IsAssignableFrom<IDictionary>(Types.IDictionary))
                        isDictionary = true;
                    else
                        isEnumerable = true;
                }
                else if (type.IsClass && isSystemNamespaceType == false)
                {
                    if (IsAssignableFrom<IIdentity>(Types.IIdentity))
                        isIdEntity = true;
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

                        object? value = null;
                        if (isDictionary)
                        {
                            string? elementJson = element.GetString();
                            if (elementJson != null)
                                value = PoulateDictionaryFromJson(type, elementJson, ref records, ref callbacks, options);
                        }
                        else if (isEnumerable)
                        {
                            string? elementJson = element.GetString();
                            if (elementJson != null)
                                value = PoulateEnumerableFromJson(type, elementJson, ref records, ref callbacks, options);
                        }
                        else
                        {
                            string? elementJson = element.GetString();
                            if (elementJson != null)
                                value = PoulateFromElement(type, elementJson, ref records, ref callbacks, options);
                        }
                           
                        if (value == null)
                            continue;

                        setter(obj, value);
                    }
                    catch (Exception ex)
                    {
                        //TODO: 이후 로그 작성 필요
                        Debug.WriteLine($"[Deserialize Error] {ex.Message}");
                        if (Debugger.IsAttached)
                            Debugger.Break();
                    }
                }
            }

            if (obj is ISerializationCallback target)
            {
                callbacks.Add(target);
            }
        }

        private static object? PoulateFromElement(Type type, string json, ref List<IdEntityRecord> records, ref List<ISerializationCallback> callbacks, JsonSerializerOptions options)
        {
            object? objectInstance = null;
            if (type.IsClass && Member.HasAttribute<SerializableClassAttribute>(GetTypeAttributes(type)))
            {
                DefaultConstructor? constructor = GetDefaultConstructor(type);
                if (constructor == null)
                {
                    //TODO: 이후 로그 작성 필요
                    Debug.WriteLine($"[Deserialize Error] {type.Name} must have a default constructor.");
                    if (Debugger.IsAttached)
                        Debugger.Break();
                    return null;
                }
                objectInstance = constructor();
                if (objectInstance == null)
                    return null;

                PoulateFromJson(objectInstance, json, ref records, ref callbacks, options);
            }
            else
            {
                try
                {
                    JsonElement element = JsonSerializer.Deserialize<JsonElement>(json, options);
                    objectInstance = element.Deserialize(type, options);
                }
                catch (Exception ex)
                {
                    //TODO: 이후 로그 작성 필요
                    Debug.WriteLine($"[Deserialize Error] {ex.Message}");
                    if (Debugger.IsAttached)
                        Debugger.Break();
                }
            }
            return objectInstance;
        }

        internal static object? PoulateDictionaryFromJson(Type type, string json, ref List<IdEntityRecord> records, ref List<ISerializationCallback> callbacks, JsonSerializerOptions options)
        {
            try
            {
                List<KeyValuePair<string, string>>? deserializeTemp = JsonSerializer.Deserialize<List<KeyValuePair<string, string>>>(json);
                if (deserializeTemp == null)
                    return null;

                DefaultConstructor? constructor = GetDefaultConstructor(type);
                if (constructor == null)
                    return null;

                object? dictionaryObject = constructor();
                if (dictionaryObject == null)
                    return null;

                if (dictionaryObject is not IDictionary dictionary)
                    return null;

                Type[] arguments = type.GetGenericArguments();
                Type keyType = arguments[0];
                Type valueType = arguments[1];
                foreach (var jsonPair in deserializeTemp)
                {
                    object? keyObject = PoulateFromElement(keyType, jsonPair.Key, ref records, ref callbacks, options);
                    object? valueObject = PoulateFromElement(valueType, jsonPair.Value, ref records, ref callbacks, options);

                    if (keyObject == null || valueObject == null)
                        continue;

                    dictionary.Add(keyObject, valueObject);
                }
                return dictionary;
            }
            catch (Exception ex)
            {
                //TODO: 이후 로그 작성 필요
                Debug.WriteLine($"[Deserialize Error] {ex.Message}");
                if (Debugger.IsAttached)
                    Debugger.Break();
            }
            return null;
        }

        internal static object? PoulateEnumerableFromJson(Type type, string json, ref List<IdEntityRecord> records, ref List<ISerializationCallback> callbacks, JsonSerializerOptions options)
        {
            try
            {
                List<string>? deserializeTemp = JsonSerializer.Deserialize<List<string>>(json);
                if (deserializeTemp == null)
                    return null;

                Type[]? elementTypes = null;
                bool isArray = type.IsArray;
                if (isArray)
                {
                    elementTypes = [type.GetElementType()!];
                }                 
                else if (type.IsGenericType)
                {
                    elementTypes = type.GetGenericArguments();
                }
                
                if (elementTypes == null)
                    return null;

                if (1 < elementTypes.Length)
                    return null;

                Type elementType = elementTypes[0];
                Type listType = typeof(List<>).MakeGenericType(elementType);

                DefaultConstructor? constructor = GetDefaultConstructor(listType);
                if (constructor == null)
                    return null;

                IList? instanceList = (IList?)constructor();
                if (instanceList == null)
                    return null;

                foreach (var rawJson in deserializeTemp)
                {
                    object? instance = PoulateFromElement(elementType, rawJson, ref records, ref callbacks, options);
                    if(instance != null)
                        instanceList.Add(instance);
                }

                if (isArray)
                {
                    Array array = Array.CreateInstance(elementType, instanceList.Count);
                    instanceList.CopyTo(array, 0);
                    return array;
                }
                else
                    return Activator.CreateInstance(type, instanceList);
            }
            catch (Exception ex)
            {
                //TODO: 이후 로그 작성 필요
                Debug.WriteLine($"[Deserialize Error] {ex.Message}");
                if (Debugger.IsAttached)
                    Debugger.Break();
            }
            return null;
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
