using System.Numerics;

namespace FixN;

public readonly partial struct Fix
{
    public static readonly Fix Epsilon = Raw(10);
    public static readonly Fix E = Raw(178145);
    public static readonly Fix Log10E = Raw(28462);
    public static readonly Fix Log2E = Raw(94548);
    public static readonly Fix Pi = Raw(205887);
    public static readonly Fix PiOverTwo = Raw(102944);
    public static readonly Fix ThreeHalvesPi = Raw(308831);
    public static readonly Fix Tau = Raw(411775);
    public static readonly Fix Root2 = Raw(92682);
    public static readonly Fix Root3 = Raw(113522);
    public static readonly Fix Deg360 = Raw(23592960);
    public static readonly Fix Deg2Rad = Raw(1144);
    public static readonly Fix Rad2Deg = Raw(3754936);
    public static readonly Fix Ln2 = Raw(45426);
    static readonly Fix _log2Max = Raw(983040);
    static readonly Fix _log2Min = Raw(-983040);
    const int LutSize = 1024;

    [MImpl(AggInline)]
    public static Fix Add(Fix a, Fix b)
    {
        // Based on: https://en.wikipedia.org/wiki/Q_(number_format)#Addition
        // with improved saturation based on: https://codereview.stackexchange.com/questions/115869/saturated-signed-addition
        var temp = a.RawValue + b.RawValue;
        const int w = (sizeof(int) << 3) - 1;
        var mask = (~(a.RawValue ^ b.RawValue) & (a.RawValue ^ temp)) >> w;
        var maxMin = (temp >> w) ^ (1 << w);
        var result = (~mask & temp) + (mask & maxMin);
        return Raw(result);
    }

    [MImpl(AggInline)]
    public static Fix Subtract(Fix a, Fix b)
    {
        var temp = a.RawValue - b.RawValue;
        const int w = (sizeof(int) << 3) - 1;
        var mask = ((a.RawValue ^ b.RawValue) & (a.RawValue ^ temp)) >> w;
        var maxMin = (temp >> w) ^ (1 << w);
        var result = (~mask & temp) + (mask & maxMin);
        return Raw(result);
    }

    [MImpl(AggInline)]
    public static Fix Multiply(Fix a, Fix b)
    {
        // Based on: https://en.wikipedia.org/wiki/Q_(number_format)#Multiplication
        var temp = (long)a.RawValue * b.RawValue;
        temp += K;
        temp >>= N;
        return Saturated(temp);
    }

    [MImpl(AggInline)]
    public static Fix Divide(Fix a, Fix b)
    {
        // Based on: https://en.wikipedia.org/wiki/Q_(number_format)#Division
        ArgumentOutOfRangeException.ThrowIfZero(b.RawValue);
        var bigA = (long)a.RawValue;
        var bigB = (long)b.RawValue;
        var temp = bigA << N;
        if ((temp >= 0 && bigB >= 0) || (temp < 0 && bigB < 0))
            temp += bigB / 2;
        else
            temp -= bigB / 2;

        temp /= bigB;
        return Saturated(temp);
    }

    [MImpl(AggInline)]
    public static Fix Modulo(Fix a, Fix b) =>
        // Overflow checks based on: https://stackoverflow.com/questions/19285163/does-modulus-overflow
        b.RawValue == 0 || (a.RawValue is int.MinValue && b.RawValue is -1) ? Zero : Raw(a.RawValue % b.RawValue);

    [MImpl(AggInline)]
    public static Fix Negate(Fix f)
    {
        var s = f.RawValue >> (S - 1);
        var raw = -f.RawValue;
        var sr = raw >> (S - 1);
        // Branchless saturation - the only input that can overflow is MinValue
        // as there is no positive equivalent, in this case saturate to MaxValue.
        raw = (raw & ~(sr & s)) | (sr & s & int.MaxValue);
        return Raw(raw);
    }

    [MImpl(AggInline)]
    public static Fix Abs(Fix f)
    {
        // https://www.chessprogramming.org/Avoiding_Branches
        // http://www.strchr.com/optimized_abs_function
        var result = f.RawValue;
        var s = result >> (S - 1);
        result ^= s;
        result -= s;
        var sr = result >> (S - 1);
        // Branchless saturation - the only input that can overflow is MinValue
        // as there is no positive equivalent, in this case saturate to MaxValue.
        result = (result & ~(sr & s)) | (sr & s & int.MaxValue);
        return Raw(result);
    }

    [MImpl(AggInline)]
    public static Fix Diff(Fix a, Fix b) => Abs(a - b);

    [MImpl(AggInline)]
    public static int Sign(Fix value) =>
        value.RawValue switch
        {
            > 0 => 1,
            < 0 => -1,
            _ => 0,
        };

    [MImpl(AggInline)]
    public static Fix Round(Fix value)
    {
        const int mid = 0x8000;
        var low = value.Lo;
        var integral = Floor(value);
        return low switch
        {
            < mid => integral,
            > mid => integral + One,
            _ => (integral.RawValue & D) is 0 ? integral : integral + One,
        };
    }

    [MImpl(AggInline)]
    public static Fix Round(Fix value, int digits)
    {
        if (digits <= 0) return Round(value);
        var scale = (Fix)Math.Pow(10, digits);
        return Round(value * scale) / scale;
    }

    [MImpl(AggInline)]
    public static Fix Floor(Fix value) => new(value.Hi);

    [MImpl(AggInline)]
    public static Fix Ceiling(Fix value) => IsInteger(value) ? value : Floor(value) + One;

    [MImpl(AggInline)]
    public static Fix Max(Fix x, Fix y) => x >= y ? x : y;

    [MImpl(AggInline)]
    public static Fix Min(Fix x, Fix y) => x <= y ? x : y;

    [MImpl(AggInline)]
    public static bool Approximately(Fix a, Fix b, Fix epsilon) => Abs(a - b) < epsilon;

    [MImpl(AggInline)]
    public static bool Approximately(Fix a, Fix b) => Approximately(a, b, Epsilon);

    [MImpl(AggInline)]
    public static Fix Clamp(Fix value, Fix min, Fix max)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    [MImpl(AggInline)]
    public static Fix Clamp<T>(Fix value) where T : INumberBase<T>, IMinMaxValue<T> =>
        Clamp(value, short.CreateSaturating(T.MinValue), short.CreateSaturating(T.MaxValue));

    [MImpl(AggInline)]
    public static Fix SnapZero(Fix value, Fix epsilon) => Approximately(value, Zero, epsilon) ? Zero : value;

    [MImpl(AggInline)]
    public static Fix SnapZero(Fix value) => SnapZero(value, Epsilon);

    [MImpl(AggInline)]
    public static Fix Lerp(Fix a, Fix b, Fix t) => a + ((b - a) * t);

    [MImpl(AggInline)]
    public static Fix Lerp(Fix a, Fix b, Fix total, Fix current) => Lerp(a, b, current / total);

    [MImpl(AggInline)]
    public static Fix Remap(
        Fix value,
        Fix inMin, Fix inMax,
        Fix outMin, Fix outMax
    )
    {
        if (inMin == inMax) return outMin;
        var t = (value - inMin) / (inMax - inMin);
        t = Clamp(t, Zero, One);
        return outMin + (t * (outMax - outMin));
    }

    [MImpl(AggInline)]
    public static Fix SmoothStep(Fix a, Fix b, Fix t)
    {
        t = Clamp(t, Zero, One);
        t = t * t * (Three - (Two * t));
        return a + ((b - a) * t);
    }

    [MImpl(AggInline)]
    public static Fix Sqrt(Fix n)
    {
        // https://groups.google.com/forum/?hl=fr%05aacf5997b615c37&fromgroups#!topic/comp.lang.c/IpwKbw0MAxw/discussion
        ArgumentOutOfRangeException.ThrowIfNegative(n.RawValue);

        // http://www.thealmightyguru.com/Pointless/PowersOf2.html
        const uint highestTestBit = 1u << 30;
        const uint lowestTestBit = 1u << (N / 2);

        var remainder = (uint)n.RawValue;
        var bit = highestTestBit;
        var root = 0u;
        while (bit >= lowestTestBit)
        {
            var trial = root + bit;
            if (remainder >= trial)
            {
                remainder -= trial;
                root = trial + bit;
            }

            remainder <<= 1;
            bit >>= 1;
        }

        root >>= N / 2;
        return Raw((int)root);
    }

    [MImpl(AggInline)]
    public static Fix Pow(Fix n, int exp)
    {
        if (n.IsZero())
            return Zero;

        switch (exp)
        {
            case 0:
                return One;
            case < 0:
                return One / Pow(n, -exp);
        }

        var result = One;
        while (exp > 0)
        {
            if ((exp & 1) is not 0)
                result *= n;

            n *= n;
            exp >>= 1;
        }

        return result;
    }

    [MImpl(AggInline)]
    public static Fix Pow(Fix n, Fix exp)
    {
        if (IsInteger(exp)) return Pow(n, exp.ToInt());
        if (exp.IsZero()) return One;
        if (exp < Zero) return One / Pow(n, -exp);
        return Exp2(exp * Log2(n));
    }

    [MImpl(AggInline)]
    public static Fix Log2(Fix n)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(n.RawValue);
        var fracBit = 1U << (N - 1);
        var result = 0L;

        var raw = n.RawValue;
        while (raw < D)
        {
            raw <<= 1;
            result -= D;
        }

        while (raw >= D << 1)
        {
            raw >>= 1;
            result += D;
        }

        var normalized = Raw(raw);
        for (var i = 0; i < N; i++)
        {
            normalized *= normalized;
            if (normalized.RawValue >= D << 1)
            {
                normalized = Raw(normalized.RawValue >> 1);
                result += fracBit;
            }

            fracBit >>= 1;
        }

        return Raw((int)result);
    }

    [MImpl(AggInline)]
    public static Fix Exp2(Fix x)
    {
        if (x.IsZero())
            return One;

        var neg = x < Zero;
        if (neg) x = -x;

        // Saturation limits for Q16.16
        if (x == One)
            return neg ? Half : Two;

        if (x >= _log2Max)
            return neg ? One / MaxValue : MaxValue;

        if (x <= _log2Min)
            return neg ? MaxValue : Zero;

        /* The algorithm is based on the power series for exp(x):
         * http://en.wikipedia.org/wiki/Exponential_function#Formal_definition
         * From term n, we get term n+1 by multiplying with x/n.
         * When the sum term drops to zero, we can stop summing. */
        var integer = x.ToInt();
        x = Raw(x.RawValue & F);
        var result = One;
        var term = One;
        var i = 1;

        Fix y = x * Ln2;
        while (term.IsNonZero())
        {
            term = term * y / i;
            result += term;
            i++;
        }

        result = Raw(Saturate((long)result.RawValue << integer));
        if (neg) result = One / result;

        return result;
    }

    [MImpl(AggInline)]
    public static Fix Cos(Fix rad) => Sin(PiOverTwo - rad);

    [MImpl(AggInline)]
    public static Fix Sin(Fix rad)
    {
        rad %= Tau;
        if (rad < Zero) rad += Tau;
        var negate = false;
        if (rad > Pi)
        {
            negate = true;
            rad -= Pi;
        }

        if (rad > PiOverTwo)
            rad = Pi - rad;

        // LUT Lookup
        var scaled = (((long)rad.RawValue * LutSize) << N) / PiOverTwo.RawValue;
        var index = (int)(scaled >> N);
        Fix result;
        if (index >= LutSize)
        {
            result = Raw(_sinLut[LutSize]);
        }
        else
        {
            var frac = (int)(scaled & F);
            var a = _sinLut[index];
            var b = _sinLut[index + 1];
            var lerp = a + (int)(((long)(b - a) * frac) >> N);
            result = Raw(lerp);
        }

        return negate ? -result : result;
    }

    [MImpl(AggInline)]
    public static (Fix Sin, Fix Cos) SinCos(Fix rad)
    {
        rad %= Tau;
        if (rad < Zero)
            rad += Tau;

        byte quadrant;
        Fix local;

        if (rad < PiOverTwo)
        {
            quadrant = 0;
            local = rad;
        }
        else if (rad < Pi)
        {
            quadrant = 1;
            local = Pi - rad;
        }
        else if (rad < ThreeHalvesPi)
        {
            quadrant = 2;
            local = rad - Pi;
        }
        else
        {
            quadrant = 3;
            local = Tau - rad;
        }

        var scaled = (((long)local.RawValue * LutSize) << N) / PiOverTwo.RawValue;
        var index = (int)(scaled >> N);
        if (index >= LutSize) index = LutSize - 1;
        var frac = (int)(scaled & F);

        var a = _sinLut[index];
        var b = _sinLut[index + 1];
        var sinRaw = a + (int)(((long)(b - a) * frac) >> N);

        index = LutSize - index;
        a = _sinLut[index];
        b = _sinLut[index - 1];
        var cosRaw = a + (int)(((long)(b - a) * frac) >> N);

        var sin = Raw(sinRaw);
        var cos = Raw(cosRaw);

        return quadrant switch
        {
            0 => (sin, cos),
            1 => (sin, -cos),
            2 => (-sin, -cos),
            _ => (-sin, cos),
        };
    }

    [MImpl(AggInline)]
    public static Fix Tan(Fix rad)
    {
        var (s, c) = SinCos(rad);
        if (c < Epsilon && c > -Epsilon)
            return Sign(s) >= 0 ? MaxValue : MinValue;

        return s / c;
    }

    [MImpl(AggInline)]
    public static Fix Atan(Fix rad)
    {
        // approximation using http://people.math.sc.edu/girardi/m142/handouts/10sTaylorPolySeries.pdf
        // best accuracy for which falls within the range of -1 <= f <= 1, see: https://spin.atomicobject.com/2012/04/24/implementing-advanced-math-functions/
        // trig identities are used to facilitate performing the approximation within the most accurate range: https://en.wikipedia.org/wiki/Inverse_trigonometric_functions
        var temp = rad;
        var useNegativeIdentity = temp < Zero;
        if (useNegativeIdentity) temp = -temp;
        var useReciprocalIdentity = temp > One;
        if (useReciprocalIdentity) temp = One / temp;
        var tt = temp * temp;
        var numerator = temp;
        var denominator = One;
        var r = temp;

        for (var i = 0; i < S / 2; ++i)
        {
            numerator *= tt;
            denominator += Two;
            temp = numerator / denominator;
            if (temp.IsZero()) break;
            r -= temp;
            numerator *= tt;
            denominator += Two;
            temp = numerator / denominator;
            if (temp.IsZero()) break;
            r += temp;
        }

        if (useReciprocalIdentity) r = PiOverTwo - r; // arctan (f) + arctan (1/f) == π/2
        if (useNegativeIdentity) r = -r; // arctan (-f) == -arctan (f)
        return r;
    }

    [MImpl(AggInline)]
    public static Fix Atan2(Fix y, Fix x)
    {
        // https://en.wikipedia.org/wiki/Atan2
        if (x.IsPositive()) return Atan(y / x);
        if (y.IsPositive()) return PiOverTwo - Atan(x / y);
        if (y.IsNegative()) return -PiOverTwo - Atan(x / y);
        if (x.IsNegative()) return Atan(y / x) + Pi;
        return Zero;
    }

    static readonly int[] _sinLut =
    [
        0x00000, 0x00065, 0x000C9, 0x0012E, 0x00192, 0x001F7, 0x0025B, 0x002C0,
        0x00324, 0x00389, 0x003ED, 0x00452, 0x004B6, 0x0051B, 0x0057F, 0x005E4,
        0x00648, 0x006AD, 0x00711, 0x00776, 0x007DA, 0x0083F, 0x008A3, 0x00908,
        0x0096C, 0x009D1, 0x00A35, 0x00A9A, 0x00AFE, 0x00B62, 0x00BC7, 0x00C2B,
        0x00C90, 0x00CF4, 0x00D59, 0x00DBD, 0x00E21, 0x00E86, 0x00EEA, 0x00F4E,
        0x00FB3, 0x01017, 0x0107B, 0x010E0, 0x01144, 0x011A8, 0x0120D, 0x01271,
        0x012D5, 0x01339, 0x0139E, 0x01402, 0x01466, 0x014CA, 0x0152E, 0x01593,
        0x015F7, 0x0165B, 0x016BF, 0x01723, 0x01787, 0x017EB, 0x01850, 0x018B4,
        0x01918, 0x0197C, 0x019E0, 0x01A44, 0x01AA8, 0x01B0C, 0x01B70, 0x01BD4,
        0x01C38, 0x01C9B, 0x01CFF, 0x01D63, 0x01DC7, 0x01E2B, 0x01E8F, 0x01EF3,
        0x01F56, 0x01FBA, 0x0201E, 0x02082, 0x020E5, 0x02149, 0x021AD, 0x02210,
        0x02274, 0x022D7, 0x0233B, 0x0239F, 0x02402, 0x02466, 0x024C9, 0x0252D,
        0x02590, 0x025F4, 0x02657, 0x026BA, 0x0271E, 0x02781, 0x027E4, 0x02848,
        0x028AB, 0x0290E, 0x02971, 0x029D5, 0x02A38, 0x02A9B, 0x02AFE, 0x02B61,
        0x02BC4, 0x02C27, 0x02C8A, 0x02CED, 0x02D50, 0x02DB3, 0x02E16, 0x02E79,
        0x02EDC, 0x02F3F, 0x02FA1, 0x03004, 0x03067, 0x030CA, 0x0312C, 0x0318F,
        0x031F1, 0x03254, 0x032B7, 0x03319, 0x0337C, 0x033DE, 0x03440, 0x034A3,
        0x03505, 0x03568, 0x035CA, 0x0362C, 0x0368E, 0x036F1, 0x03753, 0x037B5,
        0x03817, 0x03879, 0x038DB, 0x0393D, 0x0399F, 0x03A01, 0x03A63, 0x03AC5,
        0x03B27, 0x03B88, 0x03BEA, 0x03C4C, 0x03CAE, 0x03D0F, 0x03D71, 0x03DD2,
        0x03E34, 0x03E95, 0x03EF7, 0x03F58, 0x03FBA, 0x0401B, 0x0407C, 0x040DE,
        0x0413F, 0x041A0, 0x04201, 0x04262, 0x042C3, 0x04324, 0x04385, 0x043E6,
        0x04447, 0x044A8, 0x04509, 0x0456A, 0x045CB, 0x0462B, 0x0468C, 0x046EC,
        0x0474D, 0x047AE, 0x0480E, 0x0486F, 0x048CF, 0x0492F, 0x04990, 0x049F0,
        0x04A50, 0x04AB0, 0x04B10, 0x04B71, 0x04BD1, 0x04C31, 0x04C90, 0x04CF0,
        0x04D50, 0x04DB0, 0x04E10, 0x04E70, 0x04ECF, 0x04F2F, 0x04F8E, 0x04FEE,
        0x0504D, 0x050AD, 0x0510C, 0x0516C, 0x051CB, 0x0522A, 0x05289, 0x052E8,
        0x05348, 0x053A7, 0x05406, 0x05464, 0x054C3, 0x05522, 0x05581, 0x055E0,
        0x0563E, 0x0569D, 0x056FC, 0x0575A, 0x057B9, 0x05817, 0x05875, 0x058D4,
        0x05932, 0x05990, 0x059EE, 0x05A4C, 0x05AAA, 0x05B08, 0x05B66, 0x05BC4,
        0x05C22, 0x05C80, 0x05CDE, 0x05D3B, 0x05D99, 0x05DF6, 0x05E54, 0x05EB1,
        0x05F0F, 0x05F6C, 0x05FC9, 0x06026, 0x06084, 0x060E1, 0x0613E, 0x0619B,
        0x061F8, 0x06254, 0x062B1, 0x0630E, 0x0636B, 0x063C7, 0x06424, 0x06480,
        0x064DD, 0x06539, 0x06595, 0x065F2, 0x0664E, 0x066AA, 0x06706, 0x06762,
        0x067BE, 0x0681A, 0x06876, 0x068D1, 0x0692D, 0x06989, 0x069E4, 0x06A40,
        0x06A9B, 0x06AF6, 0x06B52, 0x06BAD, 0x06C08, 0x06C63, 0x06CBE, 0x06D19,
        0x06D74, 0x06DCF, 0x06E2A, 0x06E85, 0x06EDF, 0x06F3A, 0x06F94, 0x06FEF,
        0x07049, 0x070A3, 0x070FE, 0x07158, 0x071B2, 0x0720C, 0x07266, 0x072C0,
        0x0731A, 0x07373, 0x073CD, 0x07427, 0x07480, 0x074DA, 0x07533, 0x0758D,
        0x075E6, 0x0763F, 0x07698, 0x076F1, 0x0774A, 0x077A3, 0x077FC, 0x07855,
        0x078AD, 0x07906, 0x0795F, 0x079B7, 0x07A10, 0x07A68, 0x07AC0, 0x07B18,
        0x07B70, 0x07BC8, 0x07C20, 0x07C78, 0x07CD0, 0x07D28, 0x07D7F, 0x07DD7,
        0x07E2F, 0x07E86, 0x07EDD, 0x07F35, 0x07F8C, 0x07FE3, 0x0803A, 0x08091,
        0x080E8, 0x0813F, 0x08195, 0x081EC, 0x08243, 0x08299, 0x082F0, 0x08346,
        0x0839C, 0x083F2, 0x08449, 0x0849F, 0x084F5, 0x0854A, 0x085A0, 0x085F6,
        0x0864C, 0x086A1, 0x086F7, 0x0874C, 0x087A1, 0x087F6, 0x0884C, 0x088A1,
        0x088F6, 0x0894A, 0x0899F, 0x089F4, 0x08A49, 0x08A9D, 0x08AF2, 0x08B46,
        0x08B9A, 0x08BEF, 0x08C43, 0x08C97, 0x08CEB, 0x08D3F, 0x08D93, 0x08DE6,
        0x08E3A, 0x08E8D, 0x08EE1, 0x08F34, 0x08F88, 0x08FDB, 0x0902E, 0x09081,
        0x090D4, 0x09127, 0x09179, 0x091CC, 0x0921F, 0x09271, 0x092C4, 0x09316,
        0x09368, 0x093BA, 0x0940C, 0x0945E, 0x094B0, 0x09502, 0x09554, 0x095A5,
        0x095F7, 0x09648, 0x0969A, 0x096EB, 0x0973C, 0x0978D, 0x097DE, 0x0982F,
        0x09880, 0x098D0, 0x09921, 0x09972, 0x099C2, 0x09A12, 0x09A63, 0x09AB3,
        0x09B03, 0x09B53, 0x09BA3, 0x09BF2, 0x09C42, 0x09C92, 0x09CE1, 0x09D31,
        0x09D80, 0x09DCF, 0x09E1E, 0x09E6D, 0x09EBC, 0x09F0B, 0x09F5A, 0x09FA8,
        0x09FF7, 0x0A045, 0x0A094, 0x0A0E2, 0x0A130, 0x0A17E, 0x0A1CC, 0x0A21A,
        0x0A268, 0x0A2B5, 0x0A303, 0x0A350, 0x0A39E, 0x0A3EB, 0x0A438, 0x0A485,
        0x0A4D2, 0x0A51F, 0x0A56C, 0x0A5B8, 0x0A605, 0x0A652, 0x0A69E, 0x0A6EA,
        0x0A736, 0x0A782, 0x0A7CE, 0x0A81A, 0x0A866, 0x0A8B2, 0x0A8FD, 0x0A949,
        0x0A994, 0x0A9DF, 0x0AA2A, 0x0AA76, 0x0AAC1, 0x0AB0B, 0x0AB56, 0x0ABA1,
        0x0ABEB, 0x0AC36, 0x0AC80, 0x0ACCA, 0x0AD14, 0x0AD5E, 0x0ADA8, 0x0ADF2,
        0x0AE3C, 0x0AE85, 0x0AECF, 0x0AF18, 0x0AF62, 0x0AFAB, 0x0AFF4, 0x0B03D,
        0x0B086, 0x0B0CE, 0x0B117, 0x0B160, 0x0B1A8, 0x0B1F0, 0x0B239, 0x0B281,
        0x0B2C9, 0x0B311, 0x0B358, 0x0B3A0, 0x0B3E8, 0x0B42F, 0x0B477, 0x0B4BE,
        0x0B505, 0x0B54C, 0x0B593, 0x0B5DA, 0x0B620, 0x0B667, 0x0B6AD, 0x0B6F4,
        0x0B73A, 0x0B780, 0x0B7C6, 0x0B80C, 0x0B852, 0x0B898, 0x0B8DD, 0x0B923,
        0x0B968, 0x0B9AE, 0x0B9F3, 0x0BA38, 0x0BA7D, 0x0BAC1, 0x0BB06, 0x0BB4B,
        0x0BB8F, 0x0BBD4, 0x0BC18, 0x0BC5C, 0x0BCA0, 0x0BCE4, 0x0BD28, 0x0BD6B,
        0x0BDAF, 0x0BDF2, 0x0BE36, 0x0BE79, 0x0BEBC, 0x0BEFF, 0x0BF42, 0x0BF85,
        0x0BFC7, 0x0C00A, 0x0C04C, 0x0C08F, 0x0C0D1, 0x0C113, 0x0C155, 0x0C197,
        0x0C1D8, 0x0C21A, 0x0C25C, 0x0C29D, 0x0C2DE, 0x0C31F, 0x0C360, 0x0C3A1,
        0x0C3E2, 0x0C423, 0x0C463, 0x0C4A4, 0x0C4E4, 0x0C524, 0x0C564, 0x0C5A4,
        0x0C5E4, 0x0C624, 0x0C663, 0x0C6A3, 0x0C6E2, 0x0C721, 0x0C761, 0x0C7A0,
        0x0C7DE, 0x0C81D, 0x0C85C, 0x0C89A, 0x0C8D9, 0x0C917, 0x0C955, 0x0C993,
        0x0C9D1, 0x0CA0F, 0x0CA4D, 0x0CA8A, 0x0CAC7, 0x0CB05, 0x0CB42, 0x0CB7F,
        0x0CBBC, 0x0CBF9, 0x0CC35, 0x0CC72, 0x0CCAE, 0x0CCEB, 0x0CD27, 0x0CD63,
        0x0CD9F, 0x0CDDB, 0x0CE17, 0x0CE52, 0x0CE8E, 0x0CEC9, 0x0CF04, 0x0CF3F,
        0x0CF7A, 0x0CFB5, 0x0CFF0, 0x0D02A, 0x0D065, 0x0D09F, 0x0D0D9, 0x0D113,
        0x0D14D, 0x0D187, 0x0D1C1, 0x0D1FA, 0x0D234, 0x0D26D, 0x0D2A6, 0x0D2DF,
        0x0D318, 0x0D351, 0x0D38A, 0x0D3C2, 0x0D3FB, 0x0D433, 0x0D46B, 0x0D4A3,
        0x0D4DB, 0x0D513, 0x0D54B, 0x0D582, 0x0D5BA, 0x0D5F1, 0x0D628, 0x0D65F,
        0x0D696, 0x0D6CD, 0x0D703, 0x0D73A, 0x0D770, 0x0D7A6, 0x0D7DC, 0x0D812,
        0x0D848, 0x0D87E, 0x0D8B4, 0x0D8E9, 0x0D91E, 0x0D954, 0x0D989, 0x0D9BE,
        0x0D9F2, 0x0DA27, 0x0DA5C, 0x0DA90, 0x0DAC4, 0x0DAF8, 0x0DB2C, 0x0DB60,
        0x0DB94, 0x0DBC8, 0x0DBFB, 0x0DC2F, 0x0DC62, 0x0DC95, 0x0DCC8, 0x0DCFB,
        0x0DD2D, 0x0DD60, 0x0DD92, 0x0DDC5, 0x0DDF7, 0x0DE29, 0x0DE5B, 0x0DE8C,
        0x0DEBE, 0x0DEF0, 0x0DF21, 0x0DF52, 0x0DF83, 0x0DFB4, 0x0DFE5, 0x0E016,
        0x0E046, 0x0E077, 0x0E0A7, 0x0E0D7, 0x0E107, 0x0E137, 0x0E167, 0x0E196,
        0x0E1C6, 0x0E1F5, 0x0E224, 0x0E253, 0x0E282, 0x0E2B1, 0x0E2DF, 0x0E30E,
        0x0E33C, 0x0E36B, 0x0E399, 0x0E3C7, 0x0E3F4, 0x0E422, 0x0E450, 0x0E47D,
        0x0E4AA, 0x0E4D7, 0x0E504, 0x0E531, 0x0E55E, 0x0E58B, 0x0E5B7, 0x0E5E3,
        0x0E610, 0x0E63C, 0x0E667, 0x0E693, 0x0E6BF, 0x0E6EA, 0x0E716, 0x0E741,
        0x0E76C, 0x0E797, 0x0E7C2, 0x0E7EC, 0x0E817, 0x0E841, 0x0E86B, 0x0E895,
        0x0E8BF, 0x0E8E9, 0x0E913, 0x0E93C, 0x0E966, 0x0E98F, 0x0E9B8, 0x0E9E1,
        0x0EA0A, 0x0EA32, 0x0EA5B, 0x0EA83, 0x0EAAB, 0x0EAD4, 0x0EAFC, 0x0EB23,
        0x0EB4B, 0x0EB73, 0x0EB9A, 0x0EBC1, 0x0EBE8, 0x0EC0F, 0x0EC36, 0x0EC5D,
        0x0EC83, 0x0ECAA, 0x0ECD0, 0x0ECF6, 0x0ED1C, 0x0ED42, 0x0ED68, 0x0ED8D,
        0x0EDB3, 0x0EDD8, 0x0EDFD, 0x0EE22, 0x0EE47, 0x0EE6B, 0x0EE90, 0x0EEB4,
        0x0EED9, 0x0EEFD, 0x0EF21, 0x0EF45, 0x0EF68, 0x0EF8C, 0x0EFAF, 0x0EFD2,
        0x0EFF5, 0x0F018, 0x0F03B, 0x0F05E, 0x0F080, 0x0F0A3, 0x0F0C5, 0x0F0E7,
        0x0F109, 0x0F12B, 0x0F14C, 0x0F16E, 0x0F18F, 0x0F1B1, 0x0F1D2, 0x0F1F3,
        0x0F213, 0x0F234, 0x0F254, 0x0F275, 0x0F295, 0x0F2B5, 0x0F2D5, 0x0F2F5,
        0x0F314, 0x0F334, 0x0F353, 0x0F372, 0x0F391, 0x0F3B0, 0x0F3CF, 0x0F3ED,
        0x0F40C, 0x0F42A, 0x0F448, 0x0F466, 0x0F484, 0x0F4A2, 0x0F4BF, 0x0F4DD,
        0x0F4FA, 0x0F517, 0x0F534, 0x0F551, 0x0F56E, 0x0F58A, 0x0F5A6, 0x0F5C3,
        0x0F5DF, 0x0F5FB, 0x0F616, 0x0F632, 0x0F64E, 0x0F669, 0x0F684, 0x0F69F,
        0x0F6BA, 0x0F6D5, 0x0F6EF, 0x0F70A, 0x0F724, 0x0F73E, 0x0F758, 0x0F772,
        0x0F78C, 0x0F7A5, 0x0F7BF, 0x0F7D8, 0x0F7F1, 0x0F80A, 0x0F823, 0x0F83B,
        0x0F854, 0x0F86C, 0x0F885, 0x0F89D, 0x0F8B4, 0x0F8CC, 0x0F8E4, 0x0F8FB,
        0x0F913, 0x0F92A, 0x0F941, 0x0F958, 0x0F96E, 0x0F985, 0x0F99B, 0x0F9B2,
        0x0F9C8, 0x0F9DE, 0x0F9F3, 0x0FA09, 0x0FA1F, 0x0FA34, 0x0FA49, 0x0FA5E,
        0x0FA73, 0x0FA88, 0x0FA9C, 0x0FAB1, 0x0FAC5, 0x0FAD9, 0x0FAED, 0x0FB01,
        0x0FB15, 0x0FB28, 0x0FB3C, 0x0FB4F, 0x0FB62, 0x0FB75, 0x0FB88, 0x0FB9A,
        0x0FBAD, 0x0FBBF, 0x0FBD1, 0x0FBE3, 0x0FBF5, 0x0FC07, 0x0FC18, 0x0FC2A,
        0x0FC3B, 0x0FC4C, 0x0FC5D, 0x0FC6E, 0x0FC7F, 0x0FC8F, 0x0FCA0, 0x0FCB0,
        0x0FCC0, 0x0FCD0, 0x0FCDF, 0x0FCEF, 0x0FCFE, 0x0FD0E, 0x0FD1D, 0x0FD2C,
        0x0FD3B, 0x0FD49, 0x0FD58, 0x0FD66, 0x0FD74, 0x0FD83, 0x0FD90, 0x0FD9E,
        0x0FDAC, 0x0FDB9, 0x0FDC7, 0x0FDD4, 0x0FDE1, 0x0FDEE, 0x0FDFA, 0x0FE07,
        0x0FE13, 0x0FE1F, 0x0FE2B, 0x0FE37, 0x0FE43, 0x0FE4F, 0x0FE5A, 0x0FE66,
        0x0FE71, 0x0FE7C, 0x0FE87, 0x0FE91, 0x0FE9C, 0x0FEA6, 0x0FEB0, 0x0FEBA,
        0x0FEC4, 0x0FECE, 0x0FED8, 0x0FEE1, 0x0FEEB, 0x0FEF4, 0x0FEFD, 0x0FF06,
        0x0FF0E, 0x0FF17, 0x0FF1F, 0x0FF28, 0x0FF30, 0x0FF38, 0x0FF3F, 0x0FF47,
        0x0FF4E, 0x0FF56, 0x0FF5D, 0x0FF64, 0x0FF6B, 0x0FF71, 0x0FF78, 0x0FF7E,
        0x0FF85, 0x0FF8B, 0x0FF91, 0x0FF96, 0x0FF9C, 0x0FFA2, 0x0FFA7, 0x0FFAC,
        0x0FFB1, 0x0FFB6, 0x0FFBB, 0x0FFBF, 0x0FFC4, 0x0FFC8, 0x0FFCC, 0x0FFD0,
        0x0FFD4, 0x0FFD7, 0x0FFDB, 0x0FFDE, 0x0FFE1, 0x0FFE4, 0x0FFE7, 0x0FFEA,
        0x0FFEC, 0x0FFEF, 0x0FFF1, 0x0FFF3, 0x0FFF5, 0x0FFF7, 0x0FFF8, 0x0FFFA,
        0x0FFFB, 0x0FFFC, 0x0FFFD, 0x0FFFE, 0x0FFFF, 0x0FFFF, 0x10000, 0x10000,
        0x10000,
    ];
}
