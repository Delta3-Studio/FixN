using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FixN;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

/// <summary>
/// Low-precision percentage number
/// </summary>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
[JsonConverter(typeof(PercJsonConverter))]
public readonly struct Perc(byte value) :
    ISpanFormattable,
    IEquatable<Perc>,
    IComparable, IComparable<Perc>,
    IComparisonOperators<Perc, Perc, bool>
{
    public const byte FullValue = 100;
    const string DefaultFormat = "(#%);(-#%);(0%)";
    const MOpt AggInline = MOpt.AggressiveInlining;

    public static readonly Perc Full = new(FullValue);
    public static readonly Perc Zero = new(0);
    public static readonly Perc One = new(1);
    public static readonly Perc Half = new(50);

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    readonly byte value = value;

    public Perc(int value) : this(Saturate(value)) { }
    public Perc(Fix value) : this(value.ToInt()) { }
    public Perc(double value) : this((int)Math.Round(value)) { }
    public Perc(float value) : this((int)MathF.Round(value)) { }
    public Perc(Half value) : this((int)System.Half.Round(value)) { }
    [MImpl(AggInline)] static byte Saturate(int value) => (byte)Math.Clamp(value, byte.MinValue, byte.MaxValue);

    [MImpl(AggInline)] public static Perc FromUnit(float n) => new((int)MathF.Round(n * FullValue));
    [MImpl(AggInline)] public static Perc FromUnit(double n) => new((int)Math.Round(n * FullValue));
    [MImpl(AggInline)] public static Perc FromUnit(Fix n) => new((int)Fix.Round(n * Fix.OneHundred));

    [MImpl(AggInline)]
    public static Perc Clamped(int value, int min = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(min);
        return new(Math.Clamp(value, min, FullValue));
    }

    [MImpl(AggInline)] public static Perc Clamp(Perc value, Perc min, Perc max) =>
        new(byte.Clamp(value.value, min.value, max.value));

    [MImpl(AggInline)] public static Perc Clamp(Perc value, Perc min) => Clamp(value, min, Full);
    [MImpl(AggInline)] public static Perc Clamp(Perc value) => Clamp(value, Zero, Full);
    [MImpl(AggInline)] public static Perc Normalize(Perc p) => new(Normalize(p.value));
    [MImpl(AggInline)] public static int Normalize(byte value) => byte.Clamp(value, 0, FullValue);
    [MImpl(AggInline)] public static int Normalize(int value) => Math.Clamp(value, 0, FullValue);
    [MImpl(AggInline)] public static double Normalize(double value) => Math.Clamp(value, 0, FullValue);
    [MImpl(AggInline)] public static Perc Max(Perc x, Perc y) => x >= y ? x : y;
    [MImpl(AggInline)] public static Perc Min(Perc x, Perc y) => x <= y ? x : y;
    [MImpl(AggInline)] public static Perc Invert(int p) => new(FullValue - p);
    [MImpl(AggInline)] public static Perc Invert(Perc p) => Invert(p.value);
    [MImpl(AggInline)] public static int Slice(int value, int percentage) => (int)(value * (long)percentage / 100L);
    [MImpl(AggInline)] public static int Slice(int value, Perc amount) => Slice(value, amount.value);
    [MImpl(AggInline)] public static byte Slice(byte value, Perc amount) => Saturate(Slice((int)value, amount));
    [MImpl(AggInline)] public static Fix Slice(Fix value, Perc amount) => value * amount.ToUnitFix();
    [MImpl(AggInline)] public static float Slice(float value, Perc amount) => value * amount.ToUnitFloat();
    [MImpl(AggInline)] public static double Slice(double value, Perc amount) => value * amount.ToUnitDouble();
    [MImpl(AggInline)] public static Perc Scale(Perc value, int by) => new(by * value.value);
    [MImpl(AggInline)] public static Perc Scale(Perc value, Fix by) => new(by * value.value);

    [MImpl(AggInline)]
    public static int Find(int part, int total)
    {
        if (total is 0) return 0;
        var scaled = (long)part * 100;

        if (scaled >= 0)
            scaled += total / 2;
        else
            scaled -= total / 2;

        return (scaled / total) switch
        {
            < 0 => 0,
            > 100 => 100,
            var result => (int)result,
        };
    }

    [MImpl(AggInline)]
    public static double Find(double part, double total)
    {
        if (total is 0.0) return 0.0;
        return part / total * 100.0;
    }

    [MImpl(AggInline)]
    public static Fix Find(Fix part, Fix total)
    {
        if (total.IsZero()) return Fix.Zero;
        return part / total * Fix.OneHundred;
    }

    [MImpl(AggInline)] public byte ToByte() => value;
    [MImpl(AggInline)] public int ToInt() => value;
    [MImpl(AggInline)] public Fix ToUnitFix() => value / Fix.OneHundred;
    [MImpl(AggInline)] public double ToUnitDouble() => (double)value / FullValue;
    [MImpl(AggInline)] public float ToUnitFloat() => (float)value / FullValue;
    [MImpl(AggInline)] public Fix ToFix() => new(value);
    [MImpl(AggInline)] public double ToDouble() => value;
    [MImpl(AggInline)] public float ToFloat() => value;
    [MImpl(AggInline)] public Half ToHalf() => value;
    [MImpl(AggInline)] public Perc Normalize() => Normalize(this);
    [MImpl(AggInline)] public Perc Invert() => Invert(this);
    [MImpl(AggInline)] public bool IsZero() => value is 0;
    [MImpl(AggInline)] public bool IsNonZero() => value is not 0;
    [MImpl(AggInline)] public bool IsFull() => value >= FullValue;
    [MImpl(AggInline)] public bool IsNotFull() => value is not FullValue;
    [MImpl(AggInline)] public Perc Scale(int by) => Scale(this, by);
    [MImpl(AggInline)] public Perc Scale(Fix by) => Scale(this, by);

    public override string ToString() => ToString(null, null);

    public string ToString(
        [StringSyntax(StringSyntaxAttribute.NumericFormat)]
        string? format) =>
        ToString(format, null);

    public string ToString(
        [StringSyntax(StringSyntaxAttribute.NumericFormat)]
        string? format,
        IFormatProvider? formatProvider
    ) => ToUnitFloat().ToString(format ?? DefaultFormat, formatProvider ?? CultureInfo.InvariantCulture);

    public bool TryFormat(
        Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider) =>
        ToUnitFloat().TryFormat(
            destination, out charsWritten,
            format.IsEmpty ? DefaultFormat : format,
            provider ?? CultureInfo.InvariantCulture
        );

    public int CompareTo(Perc other) => value.CompareTo(other.value);

    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (obj is not Perc other)
            throw new ArgumentException($"Object must be of type {nameof(Perc)}.", nameof(obj));
        return CompareTo(other);
    }

    public override int GetHashCode() => StableHash.Combine(value);

    [MImpl(AggInline)]
    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is Perc other && other.value == value;

    [MImpl(AggInline)]
    public bool Equals(Perc other) => value == other.value;

    [MImpl(AggInline)] public static bool operator ==(Perc left, Perc right) => left.Equals(right);
    [MImpl(AggInline)] public static bool operator !=(Perc left, Perc right) => !left.Equals(right);
    [MImpl(AggInline)] public static bool operator >(Perc left, Perc right) => left.value > right.value;
    [MImpl(AggInline)] public static bool operator >=(Perc left, Perc right) => left.value >= right.value;
    [MImpl(AggInline)] public static bool operator <(Perc left, Perc right) => left.value < right.value;
    [MImpl(AggInline)] public static bool operator <=(Perc left, Perc right) => left.value <= right.value;
    [MImpl(AggInline)] public static Perc operator +(Perc left, Perc right) => new(left.value + right.value);
    [MImpl(AggInline)] public static Perc operator -(Perc left, Perc right) => new(left.value - right.value);
    [MImpl(AggInline)] public static Perc operator /(Perc left, byte right) => new(left.value / right);
    [MImpl(AggInline)] public static Perc operator /(Perc left, int right) => new(left.value / right);
    [MImpl(AggInline)] public static Perc operator %(Perc left, int right) => new(left.value % right);
    [MImpl(AggInline)] public static Perc operator *(Perc left, Perc right) => new(Slice(right.value, left));
    [MImpl(AggInline)] public static byte operator *(Perc left, byte right) => Slice(right, left);
    [MImpl(AggInline)] public static int operator *(Perc left, int right) => Slice(right, left);
    [MImpl(AggInline)] public static Fix operator *(Perc left, Fix right) => Slice(right, left);
    [MImpl(AggInline)] public static float operator *(Perc left, float right) => Slice(right, left);
    [MImpl(AggInline)] public static double operator *(Perc left, double right) => Slice(right, left);
    [MImpl(AggInline)] public static explicit operator byte(Perc p) => p.ToByte();
    [MImpl(AggInline)] public static explicit operator int(Perc p) => p.ToInt();
    [MImpl(AggInline)] public static explicit operator Fix(Perc p) => p.ToFix();
    [MImpl(AggInline)] public static explicit operator double(Perc p) => p.ToDouble();
    [MImpl(AggInline)] public static explicit operator float(Perc p) => p.ToFloat();
    [MImpl(AggInline)] public static explicit operator Half(Perc p) => p.ToHalf();
    [MImpl(AggInline)] public static explicit operator Perc(byte v) => new(v);
    [MImpl(AggInline)] public static explicit operator Perc(int v) => new(v);
    [MImpl(AggInline)] public static explicit operator Perc(Fix v) => new(v);
    [MImpl(AggInline)] public static explicit operator Perc(Half v) => new(v);
    [MImpl(AggInline)] public static explicit operator Perc(float v) => new(v);
    [MImpl(AggInline)] public static explicit operator Perc(double v) => new(v);

    sealed class PercJsonConverter : JsonConverter<Perc>
    {
        public override Perc Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => Unsafe.BitCast<byte, Perc>(reader.GetByte());

        public override void Write(Utf8JsonWriter writer, Perc value, JsonSerializerOptions options) =>
            writer.WriteNumberValue(Unsafe.As<Perc, byte>(ref value));
    }
}
