// DateOnlySerializers.cs
//
// © 2021 Espresso News. All rights reserved.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace HttpProgramming.ManualRestApi;

public class DateOnlyJSONSerializer : JsonConverter<DateOnly>
{
    private readonly string defaultDateFormat = "yyyy/MM/dd";
    private readonly string dateFormat;

    public DateOnlyJSONSerializer()
    {
        dateFormat = defaultDateFormat;
    }

    public DateOnlyJSONSerializer(string dateFormat)
    {
        this.dateFormat = dateFormat;
    }

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return DateOnly.ParseExact(value!, dateFormat);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        var serializedValue = value.ToString(dateFormat);
        writer.WriteStringValue(serializedValue);
    }
}