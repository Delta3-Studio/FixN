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
[JsonConverter(typeof(Vec3JsonConverter))]
public struct Vec3(Fix x, Fix y, Fix z) :
    IEquatable<Vec3>,
    ISpanFormattable,
    IEqualityOperators<Vec3, Vec3, bool>
{
    const MOpt AggInline = MOpt.AggressiveInlining;
    public static readonly Vec3 Zero = new(Fix.Zero);
    public static readonly Vec3 One = new(Fix.One);
    public static readonly Vec3 MaxValue = new(Fix.MaxValue);
    public static readonly Vec3 MinValue = new(Fix.MinValue);
    public static readonly Vec3 UnitX = new(Fix.One, Fix.Zero, Fix.Zero);
    public static readonly Vec3 UnitY = new(Fix.Zero, Fix.One, Fix.Zero);
    public static readonly Vec3 UnitZ = new(Fix.Zero, Fix.Zero, Fix.One);
    public static readonly Vec3 UnitXY = new(Fix.One, Fix.One, Fix.Zero);
    public static readonly Vec3 UnitXZ = new(Fix.One, Fix.Zero, Fix.One);
    public static readonly Vec3 UnitYZ = new(Fix.Zero, Fix.One, Fix.One);

    public Fix X = x;
    public Fix Y = y;
    public Fix Z = z;

    public Vec3() : this(Fix.Zero, Fix.Zero, Fix.Zero) { }
    public Vec3(Fix value) : this(value, value, value) { }
    public Vec3((Fix X, Fix Y, Fix Z) value) : this(value.X, value.Y, value.Z) { }
    public Vec3(Axis value) : this(value.UnitX, value.UnitY, value.UnitZ) { }

    public readonly void Deconstruct(out Fix x, out Fix y, out Fix z) => (x, y, z) = (X, Y, Z);

    [MImpl(AggInline)] public readonly bool IsZero() => X.IsZero() && Y.IsZero() && Z.IsZero();
    [MImpl(AggInline)] public readonly Vec3 Half() => this * Fix.Half;
    [MImpl(AggInline)] public readonly Fix Length() => Fix.Sqrt((X * X) + (Y * Y) + (Z * Z));
    [MImpl(AggInline)] public readonly Fix LengthSquared() => (X * X) + (Y * Y) + (Z * Z);
    [MImpl(AggInline)] public readonly Vec3 Normalize() => Normalize(in this);
    [MImpl(AggInline)] public readonly Vec3 Mirror() => new(-X, Y, Z);
    [MImpl(AggInline)] public readonly Vec3 Scale(Fix amount) => this * amount;

    public override readonly int GetHashCode() => StableHash.Combine(X.RawValue, Y.RawValue, Z.RawValue);
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Vec3 other && Equals(in other);
    [MImpl(AggInline)] public readonly bool Equals(in Vec3 other) => Equals(in this, in other);
    [MImpl(AggInline)] public readonly bool Equals(Vec3 other) => Equals(in this, in other);
    [MImpl(AggInline)] public static bool Equals(in Vec3 a, in Vec3 b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
    [MImpl(AggInline)] public static Vec3 Add(in Vec3 a, in Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    [MImpl(AggInline)] public static Vec3 Subtract(in Vec3 a, in Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    [MImpl(AggInline)] public static Vec3 Negate(in Vec3 value) => value * Fix.NegativeOne;
    [MImpl(AggInline)] public static Vec3 Multiply(in Vec3 a, in Vec3 b) => new(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    [MImpl(AggInline)] public static Vec3 Multiply(in Vec3 a, Fix b) => new(a.X * b, a.Y * b, a.Z * b);
    [MImpl(AggInline)] public static Vec3 Multiply(Fix a, in Vec3 b) => new(a * b.X, a * b.Y, a * b.Z);
    [MImpl(AggInline)] public static Vec3 Div(in Vec3 a, in Vec3 b) => new(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
    [MImpl(AggInline)] public static Vec3 Div(in Vec3 a, Fix b) => new(a.X / b, a.Y / b, a.Z / b);

    [MImpl(AggInline)] public static bool operator ==(in Vec3 left, in Vec3 right) => Equals(in left, in right);
    [MImpl(AggInline)] public static bool operator !=(in Vec3 left, in Vec3 right) => !Equals(in left, in right);
    [MImpl(AggInline)] public static Vec3 operator +(in Vec3 left, in Vec3 right) => Add(in left, in right);
    [MImpl(AggInline)] public static Vec3 operator -(in Vec3 left, in Vec3 right) => Subtract(in left, in right);
    [MImpl(AggInline)] public static Vec3 operator -(in Vec3 value) => Negate(in value);
    [MImpl(AggInline)] public static Vec3 operator *(in Vec3 left, in Vec3 right) => Multiply(in left, in right);
    [MImpl(AggInline)] public static Vec3 operator *(in Vec3 left, Fix right) => Multiply(in left, right);
    [MImpl(AggInline)] public static Vec3 operator *(Fix left, in Vec3 right) => Multiply(left, in right);
    [MImpl(AggInline)] public static Vec3 operator /(Vec3 left, Fix right) => Div(in left, right);
    [MImpl(AggInline)] public static Vec3 operator /(Vec3 left, Vec3 right) => Div(in left, in right);
    static bool IEqualityOperators<Vec3, Vec3, bool>.operator ==(Vec3 left, Vec3 right) => Equals(in left, in right);
    static bool IEqualityOperators<Vec3, Vec3, bool>.operator !=(Vec3 left, Vec3 right) => !Equals(in left, in right);
    [MImpl(AggInline)] public static explicit operator Vec3(Vector3 v) => new(new(v.X), new(v.Y), new(v.Z));
    [MImpl(AggInline)] public static explicit operator Vector3(Vec3 v) => new((float)v.X, (float)v.Y, (float)v.Z);

    public Vec2 XY
    {
        [MImpl(AggInline)]
        readonly get => new(X, Y);
        [MImpl(AggInline)]
        set => (X, Y) = value;
    }

    public Vec2 YX
    {
        [MImpl(AggInline)]
        readonly get => new(Y, X);
        [MImpl(AggInline)]
        set => (Y, X) = value;
    }

    public Vec2 XZ
    {
        [MImpl(AggInline)]
        readonly get => new(X, Z);
        [MImpl(AggInline)]
        set => (X, Z) = value;
    }

    public Vec2 ZX
    {
        [MImpl(AggInline)]
        readonly get => new(Z, X);
        [MImpl(AggInline)]
        set => (Z, X) = value;
    }

    public Vec2 YZ
    {
        [MImpl(AggInline)]
        readonly get => new(Y, Z);
        [MImpl(AggInline)]
        set => (Y, Z) = value;
    }

    public Vec2 ZY
    {
        [MImpl(AggInline)]
        readonly get => new(Z, Y);
        [MImpl(AggInline)]
        set => (Z, Y) = value;
    }


    [MImpl(AggInline)]
    public void Fill(in Vec3 value)
    {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
    }

    [MImpl(AggInline)]
    public void Fill(in Vec2 value)
    {
        X = value.X;
        Y = value.Y;
    }

    [MImpl(AggInline)]
    public void Fill(Fix value)
    {
        X = value;
        Y = value;
        Z = value;
    }

    [MImpl(AggInline)]
    public void Fill(Fix value, Axis axis)
    {
        if (axis.Has(Axis.X))
            X = value;
        if (axis.Has(Axis.Y))
            Y = value;
        if (axis.Has(Axis.Z))
            Z = value;
    }

    [MImpl(AggInline)]
    public static Vec3 Clamp(in Vec3 value, Fix min, Fix max) =>
        new(Fix.Clamp(value.X, min, max), Fix.Clamp(value.Y, min, max), Fix.Clamp(value.Z, min, max));

    [MImpl(AggInline)]
    public static Vec3 Clamp(in Vec3 value, in Vec3 min, in Vec3 max) =>
        new(Fix.Clamp(value.X, min.X, max.X), Fix.Clamp(value.Y, min.Y, max.Y), Fix.Clamp(value.Z, min.Z, max.Z));

    [MImpl(AggInline)]
    public static Fix DistanceSquared(Vec3 left, Vec3 right)
    {
        Fix v1 = left.X - right.X, v2 = left.Y - right.Y, v3 = left.Z - right.Z;
        return (v1 * v1) + (v2 * v2) + (v3 * v3);
    }

    [MImpl(AggInline)]
    public static Fix Distance(Vec3 left, Vec3 right)
    {
        Fix v1 = left.X - right.X, v2 = left.Y - right.Y, v3 = left.Z - right.Z;
        return Fix.Sqrt((v1 * v1) + (v2 * v2) + (v3 * v3));
    }

    [MImpl(AggInline)]
    public static Vec3 Max(in Vec3 left, in Vec3 right) =>
        new(Fix.Max(left.X, right.X), Fix.Max(left.Y, right.Y), Fix.Max(left.Z, right.Z));

    [MImpl(AggInline)]
    public static Vec3 Min(in Vec3 left, in Vec3 right) =>
        new(Fix.Min(left.X, right.X), Fix.Min(left.Y, right.Y), Fix.Min(left.Z, right.Z));

    [MImpl(AggInline)]
    public static Vec3 Sign(in Vec3 value) => new(Fix.Sign(value.X), Fix.Sign(value.Y), Fix.Sign(value.Z));

    [MImpl(AggInline)]
    public static Vec3 Abs(in Vec3 value) => new(Fix.Abs(value.X), Fix.Abs(value.Y), Fix.Abs(value.Z));

    [MImpl(AggInline)]
    public static Vec3 Round(in Vec3 value) => new(Fix.Round(value.X), Fix.Round(value.Y), Fix.Round(value.Z));

    [MImpl(AggInline)]
    public static Vec3 Round(in Vec3 value, int digits) =>
        new(Fix.Round(value.X, digits), Fix.Round(value.Y, digits), Fix.Round(value.Z, digits));

    [MImpl(AggInline)]
    public static Vec3 SmoothStep(in Vec3 a, in Vec3 b, Fix t) =>
        new(Fix.SmoothStep(a.X, b.X, t), Fix.SmoothStep(a.Y, b.Y, t), Fix.SmoothStep(a.Z, b.Z, t));

    [MImpl(AggInline)]
    public static Vec3 SmoothStep(in Vec3 a, in Vec3 b, Fix total, Fix current) =>
        SmoothStep(in a, in b, current / total);

    [MImpl(AggInline)]
    public static Vec3 Lerp(in Vec3 a, in Vec3 b, Fix t) => a + ((b - a) * t);

    [MImpl(AggInline)]
    public static Vec3 Lerp(in Vec3 a, in Vec3 b, Fix total, Fix current) => Lerp(in a, in b, current / total);

    [MImpl(AggInline)]
    public static Fix LerpInv(in Vec3 a, in Vec3 b, in Vec3 value)
    {
        var ab = b - a;
        var av = value - a;
        var denom = Dot(in ab, in ab);
        if (denom.IsZero()) return Fix.Zero;
        return Dot(in av, in ab) / denom;
    }

    [MImpl(AggInline)]
    public static Vec3 LerpClamped(in Vec3 a, in Vec3 b, Fix t) => Lerp(in a, in b, Fix.Clamp(t, Fix.Zero, Fix.One));


    [MImpl(AggInline)]
    public static Fix Dot(in Vec3 a, in Vec3 b) =>
        (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);

    [MImpl(AggInline)]
    public static Vec3 Cross(in Vec3 a, in Vec3 b) =>
        new(
            (a.Y * b.Z) - (a.Z * b.Y),
            (a.Z * b.X) - (a.X * b.Z),
            (a.X * b.Y) - (a.Y * b.X)
        );

    [MImpl(AggInline)]
    public static Vec3 Normalize(in Vec3 value)
    {
        var len = value.Length();
        return len.IsZero() ? Zero : value / len;
    }

    [MImpl(AggInline)]
    public static Vec3 Project(in Vec3 a, in Vec3 b)
    {
        var denom = Dot(b, b);
        if (denom.IsZero()) return Zero;
        return b * (Dot(a, b) / denom);
    }

    [MImpl(AggInline)]
    public static Vec3 Reflect(in Vec3 direction, in Vec3 normal) =>
        direction - (Fix.Two * Dot(direction, normal) * normal);

    [MImpl(AggInline)]
    public static Vec3 SnapZero(Vec3 value, Fix epsilon) =>
        new(Fix.SnapZero(value.X, epsilon), Fix.SnapZero(value.Y, epsilon), Fix.SnapZero(value.Z, epsilon));

    [MImpl(AggInline)]
    public static Vec3 SnapZero(Vec3 value) => SnapZero(value, Fix.Epsilon);

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
        DefaultInterpolatedStringHandler handler = new(4, 3, provider ?? CultureInfo.InvariantCulture);
        handler.AppendLiteral(prefix);
        handler.AppendFormatted(X, format);
        handler.AppendLiteral(separator);
        handler.AppendFormatted(Y, format);
        handler.AppendLiteral(separator);
        handler.AppendFormatted(Z, format);
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
        SpanStringBuilder writer = new(destination, ref charsWritten, provider ?? CultureInfo.InvariantCulture);
        return writer.Write(prefix)
               && writer.Write(X, format)
               && writer.Write(separator)
               && writer.Write(Y, format)
               && writer.Write(separator)
               && writer.Write(Z, format)
               && writer.Write(suffix);
    }

    sealed class Vec3JsonConverter : JsonConverter<Vec3>
    {
        public override Vec3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Vec3 value = new();
            if (reader.TokenType is not JsonTokenType.StartArray) throw new JsonException("Start of array expected");
            reader.Read();
            value.X = JsonSerializer.Deserialize<Fix>(ref reader, options);
            reader.Read();
            value.Y = JsonSerializer.Deserialize<Fix>(ref reader, options);
            reader.Read();
            value.Z = JsonSerializer.Deserialize<Fix>(ref reader, options);
            reader.Read();
            if (reader.TokenType is not JsonTokenType.EndArray) throw new JsonException("End of array expected");
            reader.Read();
            return value;
        }

        public override void Write(Utf8JsonWriter writer, Vec3 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            JsonSerializer.Serialize(writer, value.X, options);
            JsonSerializer.Serialize(writer, value.Y, options);
            JsonSerializer.Serialize(writer, value.Z, options);
            writer.WriteEndArray();
        }
    }
}
