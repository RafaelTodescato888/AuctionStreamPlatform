using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrossCutting.Annotations;

public class OnlyNumbers : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? value = reader.GetString();
            if (!string.IsNullOrEmpty(value))
                return value.Replace("-", "").Replace(".", "").Replace("/", "");
        }

        return reader.GetString();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
