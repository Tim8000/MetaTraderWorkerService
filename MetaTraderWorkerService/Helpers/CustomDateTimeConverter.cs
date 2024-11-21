using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private static readonly string[] SupportedFormats = new[]
    {
        "yyyy-MM-dd HH:mm:ss.fff",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd'T'HH:mm:ss.fff",
        "yyyy-MM-dd'T'HH:mm:ss",
        "yyyy-MM-dd HH:mm:ss.fffK",
        "yyyy-MM-dd'T'HH:mm:ss.fffK"
    };

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();
        if (DateTime.TryParseExact(stringValue, SupportedFormats, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dateTime))
        {
            return dateTime;
        }
        throw new JsonException($"Invalid date format. Supported formats: {string.Join(", ", SupportedFormats)}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss.fff"));
    }
}