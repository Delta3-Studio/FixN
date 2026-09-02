namespace FixN;

using System.Diagnostics;
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
[JsonConverter(typeof(Vec2JsonConverter))]
public struct Vec2(Fix x, Fix y) :
    IEquatable<Vec2>,
    ISpanFormattable,
    IEqualityOperators<Vec2, Vec2, bool>
{
    const MOpt AggInline = MOpt.AggressiveInlining;
    public static Vec2 Zero => new(Fix.Zero);
    public static Vec2 One => new(Fix.One);
    public static Vec2 MaxValue => new(Fix.MaxValue);
    public static Vec2 MinValue => new(Fix.MinValue);
    public static Vec2 UnitX => new(Fix.One, Fix.Zero);
    public static Vec2 UnitY => new(Fix.Zero, Fix.One);

    public Fix X = x;
    public Fix Y = y;

    public Vec2() : this(Fix.Zero, Fix.Zero) { }
    public Vec2(Fix value) : this(value, value) { }
    public Vec2((Fix X, Fix Y) value) : this(value.X, value.Y) { }
    public Vec2(Axis value) : this(value.UnitX, value.UnitY) { }

    public readonly void Deconstruct(out Fix x, out Fix y) => (x, y) = (X, Y);

    [MImpl(AggInline)] public readonly bool IsZero() => X.IsZero() && Y.IsZero();
    [MImpl(AggInline)] public readonly Fix Length() => Fix.Sqrt((X * X) + (Y * Y));
    [MImpl(AggInline)] public readonly Fix LengthSquared() => (X * X) + (Y * Y);
    [MImpl(AggInline)] public readonly Vec2 Normalize() => Normalize(in this);
    [MImpl(AggInline)] public readonly Vec2 Half() => this * Fix.Half;
    [MImpl(AggInline)] public readonly Vec2 Mirror() => new(-X, Y);

    public override readonly int GetHashCode() => StableHash.Combine(X.RawValue, Y.RawValue);
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Vec2 other && Equals(in other);

    [MImpl(AggInline)] public readonly bool Equals(in Vec2 other) => Equals(in this, in other);
    [MImpl(AggInline)] public readonly bool Equals(Vec2 other) => Equals(in this, in other);
    [MImpl(AggInline)] public static bool Equals(in Vec2 a, in Vec2 b) => a.X == b.X && a.Y == b.Y;
    [MImpl(AggInline)] public static Vec2 Add(in Vec2 a, in Vec2 b) => new(a.X + b.X, a.Y + b.Y);
    [MImpl(AggInline)] public static Vec2 Subtract(in Vec2 a, in Vec2 b) => new(a.X - b.X, a.Y - b.Y);
    [MImpl(AggInline)] public static Vec2 Negate(in Vec2 value) => value * Fix.NegativeOne;
    [MImpl(AggInline)] public static Vec2 Multiply(in Vec2 a, in Vec2 b) => new(a.X * b.X, a.Y * b.Y);
    [MImpl(AggInline)] public static Vec2 Multiply(in Vec2 a, Fix b) => new(a.X * b, a.Y * b);
    [MImpl(AggInline)] public static Vec2 Multiply(Fix a, in Vec2 b) => new(a * b.X, a * b.Y);
    [MImpl(AggInline)] public static Vec2 Div(in Vec2 a, in Vec2 b) => new(a.X / b.X, a.Y / b.Y);
    [MImpl(AggInline)] public static Vec2 Div(in Vec2 a, Fix b) => new(a.X / b, a.Y / b);

    [MImpl(AggInline)] public static bool operator ==(in Vec2 left, in Vec2 right) => Equals(in left, in right);
    [MImpl(AggInline)] public static bool operator !=(in Vec2 left, in Vec2 right) => !Equals(in left, in right);
    [MImpl(AggInline)] public static Vec2 operator +(in Vec2 left, in Vec2 right) => Add(in left, in right);
    [MImpl(AggInline)] public static Vec2 operator -(in Vec2 left, in Vec2 right) => Subtract(in left, in right);
    [MImpl(AggInline)] public static Vec2 operator -(in Vec2 value) => Negate(in value);
    [MImpl(AggInline)] public static Vec2 operator *(in Vec2 left, in Vec2 right) => Multiply(in left, in right);
    [MImpl(AggInline)] public static Vec2 operator *(in Vec2 left, in Fix right) => Multiply(in left, right);
    [MImpl(AggInline)] public static Vec2 operator *(in Fix left, in Vec2 right) => Multiply(left, in right);
    [MImpl(AggInline)] public static Vec2 operator /(in Vec2 left, in Vec2 right) => Div(in left, in right);
    [MImpl(AggInline)] public static Vec2 operator /(in Vec2 left, in Fix right) => Div(in left, right);
    static bool IEqualityOperators<Vec2, Vec2, bool>.operator ==(Vec2 left, Vec2 right) => Equals(in left, in right);
    static bool IEqualityOperators<Vec2, Vec2, bool>.operator !=(Vec2 left, Vec2 right) => !Equals(in left, in right);
    [MImpl(AggInline)] public static explicit operator Vector2(Vec2 v) => new((float)v.X, (float)v.Y);
    [MImpl(AggInline)] public static explicit operator Vec2(Vector2 v) => new(new(v.X), new(v.Y));

    public Vec2 YX
    {
        [MImpl(AggInline)]
        readonly get => new(Y, X);
        [MImpl(AggInline)]
        set => (Y, X) = value;
    }

    [MImpl(AggInline)]
    public void Fill(in Vec2 value)
    {
        X = value.X;
        Y = value.Y;
    }

    [MImpl(AggInline)]
    public void Fill(in Vec3 value)
    {
        X = value.X;
        Y = value.Y;
    }

    [MImpl(AggInline)]
    public void Fill(Fix value)
    {
        X = value;
        Y = value;
    }

    [MImpl(AggInline)]
    public void Fill(Fix value, Axis axis)
    {
        if (axis.Has(Axis.X))
            X = value;
        if (axis.Has(Axis.Y))
            Y = value;
    }

    [MImpl(AggInline)]
    public static Vec2 Clamp(in Vec2 value, Fix min, Fix max) =>
        new(Fix.Clamp(value.X, min, max), Fix.Clamp(value.Y, min, max));

    [MImpl(AggInline)]
    public static Vec2 Clamp(in Vec2 value, in Vec2 min, in Vec2 max) =>
        new(Fix.Clamp(value.X, min.X, max.X), Fix.Clamp(value.Y, min.Y, max.Y));

    [MImpl(AggInline)]
    public static Fix DistanceSquared(Vec2 left, Vec2 right)
    {
        Fix v1 = left.X - right.X, v2 = left.Y - right.Y;
        return (v1 * v1) + (v2 * v2);
    }

    [MImpl(AggInline)]
    public static Fix Distance(Vec2 left, Vec2 right)
    {
        Fix v1 = left.X - right.X, v2 = left.Y - right.Y;
        return Fix.Sqrt((v1 * v1) + (v2 * v2));
    }

    [MImpl(AggInline)]
    public static Vec2 Max(in Vec2 left, in Vec2 right) =>
        new(Fix.Max(left.X, right.X), Fix.Max(left.Y, right.Y));

    [MImpl(AggInline)]
    public static Vec2 Min(in Vec2 left, in Vec2 right) =>
        new(Fix.Min(left.X, right.X), Fix.Min(left.Y, right.Y));

    [MImpl(AggInline)]
    public static Vec2 Sign(in Vec2 value) => new(Fix.Sign(value.X), Fix.Sign(value.Y));

    [MImpl(AggInline)]
    public static Vec2 Abs(in Vec2 value) => new(Fix.Abs(value.X), Fix.Abs(value.Y));

    [MImpl(AggInline)]
    public static Vec2 Round(in Vec2 value) => new(Fix.Round(value.X), Fix.Round(value.Y));

    [MImpl(AggInline)]
    public static Vec2 Round(in Vec2 value, int digits) => new(Fix.Round(value.X, digits), Fix.Round(value.Y, digits));

    [MImpl(AggInline)]
    public static Vec2 Rotate(in Vec2 value, in Rotator rotator) => rotator.Apply(in value);

    [MImpl(AggInline)]
    public static Vec2 Rotate(in Vec2 value, Fix rad) => Rotate(in value, Rotator.FromRadians(rad));

    [MImpl(AggInline)]
    public static Vec2 RotateDegrees(in Vec2 v, Fix deg) => Rotate(in v, Rotator.FromDegrees(deg));

    [MImpl(AggInline)]
    public static Vec2 CirclePoint(in Rotator rotator, Fix radius) => rotator.ToVec2() * radius;

    [MImpl(AggInline)]
    public static Vec2 CirclePoint(Fix rad, Fix radius) => CirclePoint(Rotator.FromRadians(rad), radius);

    [MImpl(AggInline)]
    public static Vec2 CirclePointDegrees(Fix deg, Fix radius) => CirclePoint(Rotator.FromDegrees(deg), radius);

    [MImpl(AggInline)]
    public static Fix AngleTo(in Vec2 from, in Vec2 to) => Fix.Atan2(Cross(in from, in to), Dot(in from, in to));

    [MImpl(AggInline)]
    public static Vec2 SmoothStep(in Vec2 a, in Vec2 b, Fix t) =>
        new(Fix.SmoothStep(a.X, b.X, t), Fix.SmoothStep(a.Y, b.Y, t));

    [MImpl(AggInline)]
    public static Vec2 SmoothStep(in Vec2 a, in Vec2 b, Fix total, Fix current) =>
        SmoothStep(in a, in b, current / total);

    [MImpl(AggInline)]
    public static Vec2 Lerp(in Vec2 a, in Vec2 b, Fix t) => a + ((b - a) * t);

    [MImpl(AggInline)]
    public static Vec2 Lerp(in Vec2 a, in Vec2 b, Fix total, Fix current) => Lerp(in a, in b, current / total);

    [MImpl(AggInline)]
    public static Vec2 LerpClamped(in Vec2 a, in Vec2 b, Fix t) => Lerp(in a, in b, Fix.Clamp(t, Fix.Zero, Fix.One));

    [MImpl(AggInline)]
    public static Fix LerpInv(in Vec2 a, in Vec2 b, in Vec2 value)
    {
        var ab = b - a;
        var av = value - a;
        var denom = Dot(in ab, in ab);
        if (denom.IsZero()) return Fix.Zero;
        return Dot(in av, in ab) / denom;
    }

    [MImpl(AggInline)]
    public static Vec2 Slerp(in Vec2 from, in Vec2 to, Fix t)
    {
        var s1 = from.LengthSquared();
        var s2 = to.LengthSquared();
        if (s1.IsZero() || s2.IsZero())
            return Lerp(from, to, t);

        var angle = AngleTo(in from, in to);
        if (Fix.Abs(angle) <= Fix.Epsilon)
            return Lerp(from, to, t);

        var len1 = Fix.Sqrt(s1);
        var len2 = Fix.Sqrt(s2);
        var rot = Rotator.FromRadians(angle * t);
        var dir = rot.Apply(in from);
        var step = Fix.Lerp(len1, len2, t);
        return dir * (step / len1);
    }

    [MImpl(AggInline)]
    public static Fix Dot(in Vec2 a, in Vec2 b) => (a.X * b.X) + (a.Y * b.Y);

    [MImpl(AggInline)]
    public static Fix Cross(in Vec2 a, in Vec2 b) => (a.X * b.Y) - (a.Y * b.X);

    [MImpl(AggInline)]
    public static Vec2 Normalize(in Vec2 value)
    {
        var len = value.Length();
        return len.IsZero() ? Zero : value / len;
    }

    [MImpl(AggInline)]
    public static Vec2 Project(in Vec2 a, in Vec2 b)
    {
        var denom = b.LengthSquared();
        if (denom.IsZero()) return Zero;
        return b * (Dot(a, b) / denom);
    }

    [MImpl(AggInline)]
    public static Vec2 Reflect(in Vec2 direction, in Vec2 normal) =>
        direction - (Fix.Two * Dot(direction, normal) * normal);

    [MImpl(AggInline)]
    public static Vec2 SnapZero(Vec2 value, Fix epsilon) =>
        new(Fix.SnapZero(value.X, epsilon), Fix.SnapZero(value.Y, epsilon));

    [MImpl(AggInline)]
    public static Vec2 SnapZero(Vec2 value) => SnapZero(value, Fix.Epsilon);

    internal const string DefaultSeparator = ", ";
    internal const string DefaultPrefix = "<";
    internal const string DefaultSuffix = ">";

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
        string separator = DefaultSeparator,
        string prefix = DefaultPrefix,
        string suffix = DefaultSuffix
    )
    {
        DefaultInterpolatedStringHandler handler = new(3, 2, provider ?? CultureInfo.InvariantCulture);
        handler.AppendLiteral(prefix);
        handler.AppendFormatted(X, format);
        handler.AppendLiteral(separator);
        handler.AppendFormatted(Y, format);
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
        string separator = DefaultSeparator,
        string prefix = DefaultPrefix,
        string suffix = DefaultSuffix
    )
    {
        charsWritten = 0;
        SpanStringBuilder writer = new(destination, ref charsWritten, provider ?? CultureInfo.InvariantCulture);
        return writer.Write(prefix)
               && writer.Write(X, format)
               && writer.Write(separator)
               && writer.Write(Y, format)
               && writer.Write(suffix);
    }

    sealed class Vec2JsonConverter : JsonConverter<Vec2>
    {
        public override Vec2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Vec2 value = new();
            if (reader.TokenType is not JsonTokenType.StartArray) throw new JsonException("Start of array expected");
            reader.Read();
            value.X = JsonSerializer.Deserialize<Fix>(ref reader, options);
            reader.Read();
            value.Y = JsonSerializer.Deserialize<Fix>(ref reader, options);
            reader.Read();
            if (reader.TokenType is not JsonTokenType.EndArray) throw new JsonException("End of array expected");
            reader.Read();
            return value;
        }

        public override void Write(Utf8JsonWriter writer, Vec2 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            JsonSerializer.Serialize(writer, value.X, options);
            JsonSerializer.Serialize(writer, value.Y, options);
            writer.WriteEndArray();
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    [JsonConverter(typeof(RotatorJsonConverter))]
    [DebuggerDisplay("{ToString()} deg")]
    public readonly record struct Rotator(Fix Cos, Fix Sin) : ISpanFormattable
    {
        public static readonly Rotator Identity = new(Fix.One, Fix.Zero);

        public Rotator() : this(Fix.Zero, Fix.Zero) { }

        [MImpl(AggInline)]
        public static Rotator FromRadians(Fix rad)
        {
            var (sin, cos) = Fix.SinCos(rad);
            return new(cos, sin);
        }

        [MImpl(AggInline)]
        public static Rotator FromDegrees(Fix deg) => FromRadians(deg * Fix.Deg2Rad);

        public Fix Radians
        {
            [MImpl(AggInline)]
            get => Fix.Atan2(Sin, Cos);
        }

        public Fix DegreesSigned
        {
            [MImpl(AggInline)]
            get => Radians * Fix.Rad2Deg;
        }

        public Fix Degrees
        {
            [MImpl(AggInline)]
            get
            {
                var deg = DegreesSigned;
                return deg.IsNegative()
                    ? deg + Fix.Deg360
                    : deg;
            }
        }

        [MImpl(AggInline)]
        public Vec2 ToVec2() => new(Cos, Sin);

        [MImpl(AggInline)]
        public Vec2 Apply(in Vec2 vec) => new(
            (vec.X * Cos) - (vec.Y * Sin),
            (vec.X * Sin) + (vec.Y * Cos)
        );

        [MImpl(AggInline)]
        Fix LengthSquared() => (Cos * Cos) + (Sin * Sin);

        [MImpl(AggInline)]
        Fix Length() => Fix.Sqrt(LengthSquared());

        [MImpl(AggInline)]
        public Rotator Normalize()
        {
            var len = Length();
            return len.IsZero() ? Identity : new(Cos / len, Sin / len);
        }

        [MImpl(AggInline)]
        public bool IsNormalized(Fix epsilon)
        {
            var mag = LengthSquared();
            return Fix.Abs(mag - Fix.One) <= epsilon;
        }

        [MImpl(AggInline)]
        public bool IsNormalized() => IsNormalized(Fix.Epsilon);

        [MImpl(AggInline)]
        public bool IsZero() => Cos.IsZero() && Sin.IsZero();

        [MImpl(AggInline)]
        public bool IsIdentity() => this == Identity;

        [MImpl(AggInline)]
        public Rotator Inverse() => new(Cos, -Sin);

        [MImpl(AggInline)]
        public Rotator Mirror() => new(-Cos, Sin);

        [MImpl(AggInline)]
        public static Fix Delta(in Rotator a, in Rotator b) => (b * a.Inverse()).Radians;

        [MImpl(AggInline)]
        public static Vec2 operator *(Vec2 a, Rotator b) => b.Apply(in a);

        [MImpl(AggInline)]
        public static Vec2 operator *(Rotator a, Vec2 b) => a.Apply(in b);

        [MImpl(AggInline)]
        public static Rotator operator *(Rotator a, Rotator b) =>
            new(
                (a.Cos * b.Cos) - (a.Sin * b.Sin),
                (a.Sin * b.Cos) + (a.Cos * b.Sin)
            );

        [MImpl(AggInline)]
        public static Fix operator -(Rotator a, Rotator b) => Delta(in a, in b);

        public override string ToString() => ToString(null, null);

        public string ToString(
            [StringSyntax(StringSyntaxAttribute.NumericFormat)]
            string? format
        ) => ToString(format, null);

        public string ToString(
            [StringSyntax(StringSyntaxAttribute.NumericFormat)]
            string? format,
            IFormatProvider? formatProvider) =>
            Degrees.ToString(format ?? "F2", formatProvider);

        public bool TryFormat(
            Span<char> destination, out int charsWritten,
            [StringSyntax(StringSyntaxAttribute.NumericFormat)]
            ReadOnlySpan<char> format, IFormatProvider? provider
        ) => Degrees.TryFormat(destination, out charsWritten, format.IsEmpty ? "F2" : format, provider);

        sealed class RotatorJsonConverter : JsonConverter<Rotator>
        {
            public override Rotator Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var deg = JsonSerializer.Deserialize<Fix>(ref reader, options);
                return FromDegrees(deg);
            }

            public override void Write(Utf8JsonWriter writer, Rotator value, JsonSerializerOptions options) =>
                JsonSerializer.Serialize(writer, value.Degrees, options);
        }
    }
}
