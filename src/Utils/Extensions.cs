using System.Text.Json;

namespace FixN;

static class FixExtensions
{
    [MImpl(MOpt.AggressiveInlining)]
    public static Fix GetFix(this ref Utf8JsonReader reader)
    {
        var value = reader.GetDouble();
        return new(value);
    }

    [MImpl(MOpt.AggressiveInlining)]
    public static void WriteNumberValue(this Utf8JsonWriter writer, Fix value) =>
        writer.WriteNumberValue(value.ToDouble());
}
