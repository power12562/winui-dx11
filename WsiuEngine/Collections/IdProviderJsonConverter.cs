using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using WsiuEngine.Core.System;

namespace WsiuEngine.Collections
{
    public class IdProviderJsonConverter : JsonConverter<IdProvider>
    {
        public override IdProvider? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {        
            string? json = reader.GetString();
            if (string.IsNullOrEmpty(json))
                return null;

            IdProvider idProvider = new();
            ReflectionObject.DeserializeFromJson(idProvider, json, options);
            return idProvider;
        }

        public override void Write(Utf8JsonWriter writer, IdProvider value, JsonSerializerOptions options)
        {
            string jsonString = ReflectionObject.SerializeToJson(value, options);
            writer.WriteStringValue(jsonString);
        }
    }
}
