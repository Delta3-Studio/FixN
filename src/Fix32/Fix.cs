namespace FixN;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// 32-bit signed fixed point number Q16.16
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[DebuggerDisplay("{ToString()}")]
[JsonConverter(typeof(FixedJsonConverter))]
public readonly partial struct Fix : INumber<Fix>, IMinMaxValue<Fix>
{
    const byte N = 16;
    const int S = N * 2;
    const int D = 1 << N;
    const int F = D - 1;
    const int K = 1 << (N - 1);
    const MOpt AggInline = MOpt.AggressiveInlining;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public readonly int RawValue;

    public Fix() => RawValue = 0;
    public Fix(int v) => RawValue = v << N;
    public Fix(float v) => RawValue = Saturate((long)float.Round(v * D));
    public Fix(double v) => RawValue = Saturate((long)double.Round(v * D));
    public Fix(decimal v) => RawValue = Saturate((long)decimal.Round(v * D));
    public Fix(bool v) => RawValue = v ? D : 0;
    Fix(ref int rawValue) => RawValue = rawValue;

    public static readonly Fix Zero = Raw(0);
    public static readonly Fix One = Raw(D);
    public static readonly Fix Two = Raw(2 * D);
    public static readonly Fix Three = Raw(3 * D);
    public static readonly Fix Four = Raw(4 * D);
    public static readonly Fix NegativeOne = Raw(-D);
    public static readonly Fix Ten = Raw(10 * D);
    public static readonly Fix OneHundred = Raw(100 * D);
    public static readonly Fix OneThousand = Raw(1000 * D);
    public static readonly Fix Half = Raw(D / 2);
    public static readonly Fix Quarter = Raw(D / 4);
    public static readonly Fix MinValue = Raw(int.MinValue);
    public static readonly Fix MaxValue = Raw(int.MaxValue);
    public static readonly Fix MinIntegerValue = new(short.MinValue);
    public static readonly Fix MaxIntegerValue = new(short.MaxValue);

    [MImpl(AggInline)]
    public static Fix Raw(int value) => new(rawValue: ref value);

    [MImpl(AggInline)]
    public static Fix Create<T>(T value) where T : INumberBase<T>
    {
        if (typeof(T) == typeof(Fix)) return Unsafe.As<T, Fix>(ref value);
        return typeof(T) == typeof(double)
            ? new(Unsafe.As<T, double>(ref value))
            : new(double.CreateSaturating(value));
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int Hi
    {
        [MImpl(AggInline)]
        get => RawValue >> N;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int Lo
    {
        [MImpl(AggInline)]
        get => RawValue - (Hi << N);
    }

    [MImpl(AggInline)]
    public bool HasFlag(int flag) => (RawValue & flag) == flag;

    [MImpl(AggInline)]
    public bool HasFlag(Fix value) => HasFlag(value.RawValue);

    [MImpl(AggInline)]
    public bool HasAnyFlag(int flag) => (RawValue & flag) is not 0;

    [MImpl(AggInline)]
    public bool HasAnyFlag(Fix value) => HasAnyFlag(value.RawValue);

    [MImpl(AggInline)]
    public Fix Flag(int flag) => Raw(RawValue | flag);

    [MImpl(AggInline)]
    public Fix Flag(Fix value) => Flag(value.RawValue);

    [MImpl(AggInline)]
    public Fix Unflag(int flag) => Raw(RawValue & ~flag);

    [MImpl(AggInline)]
    public Fix Unflag(Fix value) => Unflag(value.RawValue);

    [MImpl(AggInline)]
    public int ToInt() => RawValue >> N;

    [MImpl(AggInline)]
    public float ToFloat() => 1f / D * RawValue;

    [MImpl(AggInline)]
    public double ToDouble() => 1d / D * RawValue;

    [MImpl(AggInline)]
    public decimal ToDecimal() => 1m / D * RawValue;

    [MImpl(AggInline)]
    public bool ToBool() => IsNonZero();

    [MImpl(AggInline)]
    public bool IsZero() => RawValue is 0;

    [MImpl(AggInline)]
    public bool IsNonZero() => RawValue is not 0;

    [MImpl(AggInline)]
    public bool IsNegative() => RawValue < 0;

    [MImpl(AggInline)]
    public bool IsPositive() => RawValue > 0;

    [MImpl(AggInline)]
    public bool IsApproximately(Fix value, Fix epsilon) => Approximately(this, value, epsilon);

    [MImpl(AggInline)]
    public bool IsApproximately(Fix value) => Approximately(this, value);

    public override int GetHashCode() => RawValue;

    public override string ToString() => ToString(null, null);

    public string ToString(
        [StringSyntax(StringSyntaxAttribute.NumericFormat)]
        string? format) =>
        ToString(format, null);

    public string ToString(
        [StringSyntax(StringSyntaxAttribute.NumericFormat)]
        string? format,
        IFormatProvider? formatProvider
    ) => ToDouble().ToString(format, formatProvider);

    public bool TryFormat(
        Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider) =>
        ToDouble().TryFormat(destination, out charsWritten, format, provider ?? CultureInfo.InvariantCulture);

    public int CompareTo(Fix other) => RawValue.CompareTo(other.RawValue);

    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (obj is not Fix other) throw new ArgumentException($"Object must be of type {nameof(Fix)}.", nameof(obj));
        return CompareTo(other);
    }

    [MImpl(AggInline)]
    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is Fix other && other.RawValue == RawValue;

    [MImpl(AggInline)]
    public bool Equals(Fix other) => RawValue == other.RawValue;

    [MImpl(AggInline)]
    static int Saturate(long value) => (int)long.Clamp(value, int.MinValue, int.MaxValue);

    [MImpl(AggInline)]
    static Fix Saturated(long value) => Raw(Saturate(value));

    [MImpl(AggInline)]
    public static bool Gt(Fix a, Fix b) => a.RawValue > b.RawValue;

    [MImpl(AggInline)]
    public static bool Gte(Fix a, Fix b) => a.RawValue >= b.RawValue;

    [MImpl(AggInline)]
    public static bool Lt(Fix a, Fix b) => a.RawValue < b.RawValue;

    [MImpl(AggInline)]
    public static bool Lte(Fix a, Fix b) => a.RawValue <= b.RawValue;

    [MImpl(AggInline)]
    public static bool IsInteger(Fix value) => value.Lo is 0;

    [MImpl(AggInline)]
    public static bool IsZero(Fix value) => value == Zero;

    [MImpl(AggInline)]
    public static bool IsPositive(Fix value) => value > Zero;

    [MImpl(AggInline)]
    public static bool IsNegative(Fix value) => value < Zero;

    [MImpl(AggInline)]
    public static bool IsEven(Fix value) => Abs(value % Two) == Zero;

    [MImpl(AggInline)]
    public static bool IsOdd(Fix value) => Abs(value % Two) == One;

    [MImpl(AggInline)]
    public static bool IsEvenInteger(Fix value) => IsInteger(value) && IsEven(value);

    [MImpl(AggInline)]
    public static bool IsOddInteger(Fix value) => IsInteger(value) && IsOdd(value);

    public static Fix MaxMagnitude(Fix x, Fix y)
    {
        Fix ax = Abs(x);
        Fix ay = Abs(y);
        if (ax > ay) return x;
        if (ax == ay) return IsNegative(x) ? y : x;
        return y;
    }

    public static Fix MinMagnitude(Fix x, Fix y)
    {
        Fix ax = Abs(x);
        Fix ay = Abs(y);
        if (ax < ay) return x;
        if (ax == ay) return IsNegative(x) ? x : y;
        return y;
    }

    static Fix IMinMaxValue<Fix>.MaxValue => MaxValue;
    static Fix IMinMaxValue<Fix>.MinValue => MinValue;
    static Fix INumberBase<Fix>.Zero => Zero;
    static Fix INumberBase<Fix>.One => One;
    static int INumberBase<Fix>.Radix => 2;
    static Fix IAdditiveIdentity<Fix, Fix>.AdditiveIdentity => Zero;
    static Fix IMultiplicativeIdentity<Fix, Fix>.MultiplicativeIdentity => One;
    static Fix INumberBase<Fix>.MaxMagnitudeNumber(Fix x, Fix y) => MaxMagnitude(x, y);
    static Fix INumberBase<Fix>.MinMagnitudeNumber(Fix x, Fix y) => MinMagnitude(x, y);
    static bool INumberBase<Fix>.IsCanonical(Fix value) => true;
    static bool INumberBase<Fix>.IsFinite(Fix value) => true;
    static bool INumberBase<Fix>.IsInfinity(Fix value) => false;
    static bool INumberBase<Fix>.IsComplexNumber(Fix value) => false;
    static bool INumberBase<Fix>.IsImaginaryNumber(Fix value) => false;
    static bool INumberBase<Fix>.IsNaN(Fix value) => false;
    static bool INumberBase<Fix>.IsNegativeInfinity(Fix value) => false;
    static bool INumberBase<Fix>.IsPositiveInfinity(Fix value) => false;
    static bool INumberBase<Fix>.IsSubnormal(Fix value) => false;
    static bool INumberBase<Fix>.IsNormal(Fix value) => !IsZero(value);
    static bool INumberBase<Fix>.IsRealNumber(Fix value) => true;
    const NumberStyles DefaultNumberStyle = NumberStyles.Any;

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        NumberStyles style, IFormatProvider? provider,
        out Fix result
    )
    {
        var ok = double.TryParse(s, style, provider, out var d);
        result = new(d);
        return ok;
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Fix result) =>
        TryParse(s, DefaultNumberStyle, null, out result);

    public static bool TryParse(string s, out Fix result) => TryParse(s, null, out result);

    public static bool TryParse(
        ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Fix result)
    {
        var ok = double.TryParse(s, style, provider, out var d);
        result = new(d);
        return ok;
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Fix result) =>
        TryParse(s, DefaultNumberStyle, null, out result);

    public static Fix Parse(string s, NumberStyles style, IFormatProvider? provider) =>
        new(double.Parse(s, style, provider));

    public static Fix Parse(string s, IFormatProvider? provider) => Parse(s, DefaultNumberStyle, provider);

    public static Fix Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) =>
        new(double.Parse(s, style, provider));

    public static Fix Parse(ReadOnlySpan<char> s, IFormatProvider? provider) =>
        Parse(s, DefaultNumberStyle, provider);

    static bool INumberBase<Fix>.TryConvertFromChecked<TOther>(TOther value, out Fix result) =>
        TryConvertFrom(value, out result);

    public static bool TryConvertFromSaturating<TOther>(TOther value, out Fix result)
        where TOther : INumberBase<TOther> => TryConvertFrom(value, out result);

    public static bool TryConvertFromTruncating<TOther>(TOther value, out Fix result)
        where TOther : INumberBase<TOther> => TryConvertFrom(value, out result);

    public static bool TryConvertToSaturating<TOther>(Fix value, [MaybeNullWhen(false)] out TOther result)
        where TOther : INumberBase<TOther> => TryConvertTo(value, out result);

    public static bool TryConvertToTruncating<TOther>(Fix value, [MaybeNullWhen(false)] out TOther result)
        where TOther : INumberBase<TOther> => TryConvertTo(value, out result);

    public static bool TryConvertToChecked<TOther>(Fix value, [MaybeNullWhen(false)] out TOther result)
        where TOther : INumberBase<TOther> => TryConvertTo(value, out result);

    [MImpl(AggInline)] public static Fix operator +(Fix left, Fix right) => Add(left, right);
    [MImpl(AggInline)] public static Fix operator -(Fix left, Fix right) => Subtract(left, right);
    [MImpl(AggInline)] public static Fix operator /(Fix left, Fix right) => Divide(left, right);
    [MImpl(AggInline)] public static Fix operator *(Fix left, Fix right) => Multiply(left, right);
    [MImpl(AggInline)] public static Fix operator %(Fix left, Fix right) => Modulo(left, right);
    [MImpl(AggInline)] public static Fix operator +(Fix value) => value;
    [MImpl(AggInline)] public static Fix operator -(Fix value) => Negate(value);
    [MImpl(AggInline)] public static Fix operator ++(Fix value) => Add(value, One);
    [MImpl(AggInline)] public static Fix operator --(Fix value) => Subtract(value, One);
    [MImpl(AggInline)] public static bool operator ==(Fix a, Fix b) => a.Equals(b);
    [MImpl(AggInline)] public static bool operator !=(Fix a, Fix b) => !a.Equals(b);
    [MImpl(AggInline)] public static bool operator >(Fix a, Fix b) => Gt(a, b);
    [MImpl(AggInline)] public static bool operator <(Fix a, Fix b) => Lt(a, b);
    [MImpl(AggInline)] public static bool operator >=(Fix a, Fix b) => Gte(a, b);
    [MImpl(AggInline)] public static bool operator <=(Fix a, Fix b) => Lte(a, b);
    [MImpl(AggInline)] public static explicit operator int(Fix f) => f.ToInt();
    [MImpl(AggInline)] public static explicit operator float(Fix f) => f.ToFloat();
    [MImpl(AggInline)] public static explicit operator double(Fix f) => f.ToDouble();
    [MImpl(AggInline)] public static explicit operator decimal(Fix f) => f.ToDecimal();
    [MImpl(AggInline)] public static explicit operator bool(Fix f) => f.ToBool();
    [MImpl(AggInline)] public static explicit operator Fix(bool v) => new(v);
    [MImpl(AggInline)] public static implicit operator Fix(float v) => new(v);
    [MImpl(AggInline)] public static implicit operator Fix(double v) => new(v);
    [MImpl(AggInline)] public static implicit operator Fix(decimal v) => new(v);
    [MImpl(AggInline)] public static implicit operator Fix(int v) => new(v);

    [MImpl(AggInline)]
    static bool TryConvertFrom<TOther>(TOther value, out Fix result)
        where TOther : INumberBase<TOther>
    {
        if (typeof(TOther) == typeof(Half))
        {
            result = (double)Unsafe.As<TOther, Half>(ref value);
            return true;
        }

        if (typeof(TOther) == typeof(float))
        {
            result = Unsafe.As<TOther, float>(ref value);
            return true;
        }

        if (typeof(TOther) == typeof(double))
        {
            result = Unsafe.As<TOther, double>(ref value);
            return true;
        }

        if (typeof(TOther) == typeof(short))
        {
            result = Unsafe.As<TOther, short>(ref value);
            return true;
        }

        if (typeof(TOther) == typeof(int))
        {
            result = Unsafe.As<TOther, int>(ref value);
            return true;
        }

        if (typeof(TOther) == typeof(sbyte))
        {
            result = Unsafe.As<TOther, sbyte>(ref value);
            return true;
        }

        result = default;
        return false;
    }

    [MImpl(AggInline)]
    static bool TryConvertTo<TOther>(Fix value, [MaybeNullWhen(false)] out TOther result)
        where TOther : INumberBase<TOther>
    {
        if (typeof(TOther) == typeof(Half))
        {
            var actualResult = (Half)value.ToFloat();
            result = Unsafe.As<Half, TOther>(ref actualResult);
            return true;
        }

        if (typeof(TOther) == typeof(float))
        {
            var actualResult = value.ToFloat();
            result = Unsafe.As<float, TOther>(ref actualResult);
            return true;
        }

        if (typeof(TOther) == typeof(double))
        {
            var actualResult = value.ToDouble();
            result = Unsafe.As<double, TOther>(ref actualResult);
            return true;
        }

        if (typeof(TOther) == typeof(decimal))
        {
            var actualResult = value.ToDecimal();
            result = Unsafe.As<decimal, TOther>(ref actualResult);
            return true;
        }

        if (typeof(TOther) == typeof(byte))
        {
            var actualResult = (byte)Math.Clamp(value.ToInt(), byte.MinValue, byte.MaxValue);
            result = Unsafe.As<byte, TOther>(ref actualResult);
            return true;
        }

        if (typeof(TOther) == typeof(ushort))
        {
            var actualResult = (ushort)Math.Clamp(value.ToInt(), ushort.MinValue, ushort.MaxValue);
            result = Unsafe.As<ushort, TOther>(ref actualResult);
            return true;
        }

        if (typeof(TOther) == typeof(uint))
        {
            var actualResult = (uint)Math.Clamp(value.ToInt(), uint.MinValue, uint.MaxValue);
            result = Unsafe.As<uint, TOther>(ref actualResult);
            return true;
        }

        result = default;
        return false;
    }

    sealed class FixedJsonConverter : JsonConverter<Fix>
    {
        public override Fix Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            var value = reader.GetDouble();
            return new(value);
        }

        public override void Write(Utf8JsonWriter writer, Fix value, JsonSerializerOptions options) =>
            writer.WriteNumberValue(value.ToDouble());
    }
}
