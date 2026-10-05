using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ciee.Curriculos.Api.Common.Converters;

public class Iso8601UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TryGetDateTime(out var date))
        {
            return date.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(date, DateTimeKind.Utc) 
                : date.ToUniversalTime();
        }

        var str = reader.GetString();
        if (DateTime.TryParse(str, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var parsed))
        {
            return parsed;
        }

        return DateTime.UtcNow;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utcDate = value.Kind == DateTimeKind.Utc 
            ? value 
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);

        writer.WriteStringValue(utcDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}
