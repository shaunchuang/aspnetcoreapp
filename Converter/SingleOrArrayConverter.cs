// Converters/SingleOrArrayConverter.cs
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace aspnetcoreapp.Converter // 確保命名空間與專案一致
{
    public class SingleOrArrayConverter<T> : JsonConverter<List<T>>
    {
        public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var list = new List<T>();

            if (reader.TokenType == JsonTokenType.StartArray)
            {
                list = JsonSerializer.Deserialize<List<T>>(ref reader, options);
            }
            else
            {
                var item = JsonSerializer.Deserialize<T>(ref reader, options);
                if (item != null)
                {
                    list.Add(item);
                }
            }

            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
