using System.Text.Json;
using System.Text.Json.Serialization;

namespace HachijouBot.Common;

public class NullableDateConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TryGetDateTime(out var date))
        {
            return date;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is { } date)
        {
            writer.WriteStringValue(TimeZoneInfo.ConvertTimeFromUtc(date, TimeZoneInfo.Local));
            return;
        }

        writer.WriteNullValue();
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(DateTime?) == typeToConvert;
    }
}

