namespace FixN;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using MImpl = System.Runtime.CompilerServices.MethodImplAttribute;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
[JsonConverter(typeof(RectJsonConverter))]
public struct Rect(Vec2 position, Vec2 size) :
    IEquatable<Rect>,
    ISpanFormattable,
    IEqualityOperators<Rect, Rect, bool>
{
    public Rect(Fix x, Fix y, Fix width, Fix height) : this(new(x, y), new(width, height)) { }

    const MOpt AggInline = MOpt.AggressiveInlining;
    public static readonly Rect Empty;
    public Vec2 Position = position;
    public Vec2 Size = size;

    public readonly Fix X
    {
        [MImpl(AggInline)]
        get => Position.X;
    }

    public readonly Fix Y
    {
        [MImpl(AggInline)]
        get => Position.Y;
    }

    public readonly Fix Width
    {
        [MImpl(AggInline)]
        get => Size.X;
    }

    public readonly Fix Height
    {
        [MImpl(AggInline)]
        get => Size.Y;
    }

    public readonly Fix Left
    {
        [MImpl(AggInline)]
        get => X;
    }

    public readonly Fix Right
    {
        [MImpl(AggInline)]
        get => X + Width;
    }

    public readonly Fix Top
    {
        [MImpl(AggInline)]
        get => Y;
    }

    public readonly Fix Bottom
    {
        [MImpl(AggInline)]
        get => Y + Height;
    }

    public readonly Vec2 End
    {
        [MImpl(AggInline)]
        get => Position + Size;
    }

    public readonly Vec2 Center
    {
        [MImpl(AggInline)]
        get => Position + (Size * Fix.Half);
    }

    public override readonly int GetHashCode() =>
        StableHash.Combine(X.RawValue, Y.RawValue, Width.RawValue, Height.RawValue);

    public readonly bool Equals(Rect other) => Equals(in this, in other);
    public override readonly bool Equals(object? obj) => obj is Rect other && Equals(in this, in other);

    [MImpl(AggInline)]
    public static bool Equals(in Rect left, in Rect right) =>
        Vec2.Equals(in left.Position, right.Position) && Vec2.Equals(in left.Size, right.Size);

    [MImpl(AggInline)]
    public static bool operator ==(in Rect left, in Rect right) => Equals(in left, in right);

    [MImpl(AggInline)]
    public static bool operator !=(in Rect left, in Rect right) => !Equals(in left, in right);

    static bool IEqualityOperators<Rect, Rect, bool>.operator ==(Rect left, Rect right) =>
        Equals(in left, in right);

    static bool IEqualityOperators<Rect, Rect, bool>.operator !=(Rect left, Rect right) =>
        !Equals(in left, in right);

    [MImpl(AggInline)]
    public readonly bool Intersects(in Rect value) => Intersects(in this, in value);

    [MImpl(AggInline)]
    public readonly bool Intersect(in Rect value, out Rect result)
        => Intersect(in this, in value, out result);

    [MImpl(AggInline)]
    public static bool Intersects(in Rect value1, in Rect value2) =>
        value1.Left <= value2.Right &&
        value2.Left <= value1.Right &&
        value1.Top <= value2.Bottom &&
        value2.Top <= value1.Bottom;

    [MImpl(AggInline)]
    public static bool Intersect(in Rect a, in Rect b, out Rect overlap)
    {
        if (!Intersects(a, b))
        {
            overlap = Empty;
            return false;
        }

        var x = Fix.Max(a.X, b.X);
        var y = Fix.Max(a.Y, b.Y);
        var w = Fix.Min(a.X + a.Size.X, b.X + b.Size.X) - x;
        var h = Fix.Min(a.Y + a.Size.Y, b.Y + b.Size.Y) - y;
        overlap = new(x, y, w, h);
        return true;
    }

    [MImpl(AggInline)]
    public readonly bool Contains(in Rect value) =>
        X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y &&
        value.Y + value.Height <= Y + Height;

    [MImpl(AggInline)]
    public readonly bool Contains(in Vec2 value) =>
        X <= value.X && value.X <= X + Width && Y <= value.Y &&
        value.Y <= Y + Height;

    public readonly Rect Offset(in Vec2 by) => new(Position + by, Size);
    public readonly Rect Offset(Fix x, Fix y) => Offset(new(x, y));

    public readonly Rect Inflate(in Vec2 amount) =>
        new(Position - amount, Size + (amount * Fix.Two));

    public readonly Rect Inflate(Fix horizontal, Fix vertical) =>
        Inflate(new Vec2(horizontal, vertical));

    public readonly Rect Inflate(Fix by) => Inflate(new Vec2(by, by));

    public readonly Rect Mirror()
    {
        var half = Width * Fix.Half;
        var pos = new Vec2(-(Position.X + half), Position.Y);
        return new(pos.X - half, pos.Y, Width, Height);
    }

    public readonly Rect CentralizeX() => new(X - (Width * Fix.Half), Y, Width, Height);

    public static Rect Union(Rect a, Rect b)
    {
        var x1 = Fix.Min(a.X, b.X);
        var x2 = Fix.Max(a.X + a.Width, b.X + b.Width);
        var y1 = Fix.Min(a.Y, b.Y);
        var y2 = Fix.Max(a.Y + a.Height, b.Y + b.Height);
        return new(x1, y1, x2 - x1, y2 - y1);
    }

    readonly string IFormattable.ToString(
        [StringSyntax(StringSyntaxAttribute.NumericFormat)]
        string? format,
        IFormatProvider? formatProvider
    ) => ToString(format, provider: formatProvider);

    public override readonly string ToString() => ToString(null);

    public readonly string ToString(
        [StringSyntax(StringSyntaxAttribute.NumericFormat)]
        string? format,
        IFormatProvider? provider = null,
        string separator = Vec2.DefaultSeparator,
        string prefix = Vec2.DefaultPrefix,
        string suffix = Vec2.DefaultSuffix
    )
    {
        DefaultInterpolatedStringHandler handler = new(5, 4, provider ?? CultureInfo.InvariantCulture);
        handler.AppendLiteral(prefix);
        handler.AppendFormatted(X, format);
        handler.AppendLiteral(separator);
        handler.AppendFormatted(Y, format);
        handler.AppendLiteral(separator);
        handler.AppendFormatted(Width, format);
        handler.AppendLiteral(separator);
        handler.AppendFormatted(Height, format);
        handler.AppendLiteral(suffix);
        return handler.ToStringAndClear();
    }

    readonly bool ISpanFormattable.TryFormat(
        Span<char> destination, out int charsWritten,
        [StringSyntax(StringSyntaxAttribute.NumericFormat)]
        ReadOnlySpan<char> format,
        IFormatProvider? provider
    ) => TryFormat(destination, out charsWritten, format, provider);

    public readonly bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        [StringSyntax(StringSyntaxAttribute.NumericFormat)]
        ReadOnlySpan<char> format = default,
        IFormatProvider? provider = null,
        string separator = Vec2.DefaultSeparator,
        string prefix = Vec2.DefaultPrefix,
        string suffix = Vec2.DefaultSuffix
    )
    {
        charsWritten = 0;
        SpanStringBuilder writer = new(destination, ref charsWritten,
            provider ?? CultureInfo.InvariantCulture);
        return writer.Write(prefix)
               && writer.Write(X, format)
               && writer.Write(separator)
               && writer.Write(Y, format)
               && writer.Write(separator)
               && writer.Write(Width, format)
               && writer.Write(separator)
               && writer.Write(Height, format)
               && writer.Write(suffix);
    }

    sealed class RectJsonConverter : JsonConverter<Rect>
    {
        public override Rect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Rect value = new();
            if (reader.TokenType is not JsonTokenType.StartArray) throw new JsonException("Start of array expected");
            reader.Read();
            value.Position.X = JsonSerializer.Deserialize<Fix>(ref reader, options);
            reader.Read();
            value.Position.Y = JsonSerializer.Deserialize<Fix>(ref reader, options);
            reader.Read();
            value.Size.X = JsonSerializer.Deserialize<Fix>(ref reader, options);
            reader.Read();
            value.Size.Y = JsonSerializer.Deserialize<Fix>(ref reader, options);
            reader.Read();
            if (reader.TokenType is not JsonTokenType.EndArray) throw new JsonException("End of array expected");
            reader.Read();
            return value;
        }

        public override void Write(Utf8JsonWriter writer, Rect value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            JsonSerializer.Serialize(writer, value.X, options);
            JsonSerializer.Serialize(writer, value.Y, options);
            JsonSerializer.Serialize(writer, value.Width, options);
            JsonSerializer.Serialize(writer, value.Height, options);
            writer.WriteEndArray();
        }
    }
}
