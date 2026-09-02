namespace FixN.Core.Tests.Data;

using System.Text.Json;

public class FixTests
{
    [Test]
    public void ShouldSerializeJson()
    {
        Fix sut = 1.5;
        var json = JsonSerializer.Serialize(sut);
        json.Should().Be("1.5");
    }

    [Test]
    public void ShouldDeserializeJson()
    {
        const string json = "1.5";
        Fix expected = 1.5;
        JsonSerializer.Deserialize<Fix>(json).Should().Be(expected);
    }

    [Test]
    public void ShouldReturnMax() => Fix.MaxValue.RawValue.Should().Be(int.MaxValue);

    [Test]
    public void ShouldReturnMin() => Fix.MinValue.RawValue.Should().Be(int.MinValue);

    [Test]
    public void ShouldPi() => Fix.Pi.ToFloat().Should().BeApproximately(MathF.PI, 0.0001f);

    [Test]
    public void ShouldFloor()
    {
        Fix value = 1.785;
        Fix.Floor(value).Should().Be(Fix.One);
    }

    [TestCase(1.5)]
    [TestCase(1.51)]
    [TestCase(1.6)]
    public void ShouldRoundUp(double value)
    {
        var sut = (Fix)value;
        Fix.Round(sut).Should().Be(2);
    }

    [TestCase(1.499)]
    [TestCase(1.2)]
    public void ShouldRoundDown(double value)
    {
        var sut = (Fix)value;
        Fix.Round(sut).Should().Be(1.0);
    }

    [TestCase(1.785, 2, 1.79)]
    [TestCase(1.784, 2, 1.78)]
    public void ShouldRoundNDigits(double value, int digits, double result) =>
        Fix.Round(value, digits).Should().Be(result);

    [Test]
    public void ShouldNegate()
    {
        Fix value = 10;
        Fix expected = -10;
        var result = -value;
        result.Should().Be(expected);
    }

    [Test]
    public void ShouldSum()
    {
        Fix a = 1.56;
        Fix b = 1.56;
        (a + b).Should().Be(3.12);
    }

    [Test]
    public void ShouldSubtract()
    {
        Fix a = 3.12;
        Fix b = 1.56;
        (a - b).Should().Be(1.56);
    }

    [Test]
    public void ShouldSubtractNegative()
    {
        Fix a = 1.56;
        Fix b = 3.12;
        (a - b).Should().Be(-1.56);
    }

    [Test]
    public void ShouldMultiply()
    {
        Fix a = 1.56;
        (a * 2).Should().Be(3.12);
    }

    [Test]
    public void ShouldDivide()
    {
        Fix a = 3.12;
        (a / 2).Should().Be(1.56);
    }

    [Test]
    public void ShouldCompareGreater()
    {
        Fix a = 1.12;
        Fix b = 1.1;
        (a > b).Should().BeTrue();
        (a >= b).Should().BeTrue();
    }

    [Test]
    public void ShouldCompareLess()
    {
        Fix a = 1.1;
        Fix b = 1.12;
        (a < b).Should().BeTrue();
        (a <= b).Should().BeTrue();
    }

    [Test]
    public void ShouldModInt()
    {
        Fix a = 3;
        Fix b = 2;
        (a % b).Should().Be(1);
    }

    [Test]
    public void ShouldModFloat()
    {
        Fix a = 3.5;
        Fix b = 2;
        (a % b).Should().Be(1.5);
    }

    [TestCase(0.0, 0.0)]
    [TestCase(1.0, 1.0)]
    [TestCase(4.0, 2.0)]
    [TestCase(9.0, 3.0)]
    [TestCase(16.0, 4.0)]
    [TestCase(25.0, 5.0)]
    [TestCase(100.0, 10.0)]
    [TestCase(0.25, 0.5)]
    [TestCase(0.5, 0.70710678)]
    [TestCase(2.0, 1.41421356)]
    [TestCase(3.0, 1.73205080)]
    [TestCase(5.0, 2.23606798)]
    [TestCase(10.0, 3.16227766)]
    [TestCase(1000.0, 31.62277660)]
    [TestCase(0.01, 0.1)]
    [TestCase(0.001, 0.03162277)]
    [TestCase(32767.0, 181.01657382)]
    public void CalcSqrt(double value, double expected)
    {
        var actual = Fix.Sqrt(value);
        Fix expectedFix = expected;
        actual.Should().BeApproximately(expectedFix, 0.001);
    }

    [TestCase(Math.PI / 6.0, 0.86602540)]
    [TestCase(Math.PI / 4.0, 0.70710678)]
    [TestCase(Math.PI / 3.0, 0.5)]
    [TestCase(Math.PI / 2.0, 0.0)]
    [TestCase(Math.PI, -1.0)]
    [TestCase(3.0 * Math.PI / 2.0, 0.0)]
    [TestCase(2.0 * Math.PI, 1.0)]
    [TestCase(-Math.PI / 6.0, 0.86602540)]
    [TestCase(-Math.PI / 4.0, 0.70710678)]
    [TestCase(-Math.PI / 3.0, 0.5)]
    public void ShouldCos(double x, double y)
    {
        var sut = Fix.Cos(x);
        Fix expected = y;
        sut.Should().BeApproximately(expected);
    }

    [TestCase(0.0, 0.0)]
    [TestCase(Math.PI / 2.0, 1.0)]
    [TestCase(Math.PI, 0.0)]
    [TestCase(3.0 * Math.PI / 2.0, -1.0)]
    [TestCase(2.0 * Math.PI, 0.0)]
    [TestCase(Math.PI / 6.0, 0.5)]
    [TestCase(Math.PI / 4.0, 0.70710678)]
    [TestCase(Math.PI / 3.0, 0.86602540)]
    [TestCase(-Math.PI / 6.0, -0.5)]
    [TestCase(-Math.PI / 4.0, -0.70710678)]
    [TestCase(-Math.PI / 3.0, -0.86602540)]
    public void CalcSin(double rad, double expected)
    {
        var actual = Fix.Sin(rad);
        Fix expectedFix = expected;
        actual.Should().BeApproximately(expectedFix);
    }

    [TestCase(0.0, 0.0)]
    [TestCase(Math.PI / 6.0, 0.57735027)]
    [TestCase(Math.PI / 4.0, 1.0)]
    [TestCase(Math.PI / 3.0, 1.73205080)]
    [TestCase(-Math.PI / 6.0, -0.57735027)]
    [TestCase(-Math.PI / 4.0, -1.0)]
    [TestCase(-Math.PI / 3.0, -1.73205080)]
    [TestCase(Math.PI, 0.0)]
    [TestCase(2.0 * Math.PI, 0.0)]
    [TestCase(5.0 * Math.PI / 4.0, 1.0)]
    [TestCase(7.0 * Math.PI / 4.0, -1.0)]
    public void CalcTan(double rad, double expected)
    {
        var actual = Fix.Tan(rad);
        Fix expectedFix = expected;
        actual.Should().BeApproximately(expectedFix);
    }

    [TestCase(2.0, 0.0, 1.0)]
    [TestCase(2.0, 1.0, 2.0)]
    [TestCase(2.0, 2.0, 4.0)]
    [TestCase(2.0, 3.0, 8.0)]
    [TestCase(2.0, 4.0, 16.0)]
    [TestCase(2.0, -1.0, 0.5)]
    [TestCase(2.0, -2.0, 0.25)]
    [TestCase(2.0, -3.0, 0.125)]
    [TestCase(4.0, 0.5, 2.0)]
    [TestCase(9.0, 0.5, 3.0)]
    [TestCase(16.0, 0.5, 4.0)]
    [TestCase(8.0, 0.33333333, 2.0)]
    [TestCase(27.0, 0.33333333, 3.0)]
    [TestCase(2.0, 1.5, 2.82842712)]
    [TestCase(2.0, 2.5, 5.65685424)]
    [TestCase(10.0, 1.8, 63.09573445)]
    [TestCase(5.0, 2.0, 25.0)]
    [TestCase(5.0, 3.0, 125.0)]
    [TestCase(0.5, 2.0, 0.25)]
    [TestCase(0.25, 0.5, 0.5)]
    [TestCase(100.0, 0.5, 10.0)]
    [TestCase(1000.0, 0.33333333, 10.0)]
    [TestCase(1.5, 5.0, 7.59375)]
    [TestCase(1.25, 8.0, 5.96046448)]
    [TestCase(0.9, 10.0, 0.34867844)]
    public void ShouldPow(double value, double exp, double result)
    {
        var sut = Fix.Pow(value, exp);
        Fix expected = result;
        sut.Should().BeApproximately(expected, 0.001);
    }

    [TestCase(1.0, 0.0)]
    [TestCase(2.0, 1.0)]
    [TestCase(4.0, 2.0)]
    [TestCase(8.0, 3.0)]
    [TestCase(16.0, 4.0)]
    [TestCase(0.5, -1.0)]
    [TestCase(0.25, -2.0)]
    [TestCase(0.125, -3.0)]
    [TestCase(1.5, 0.58496250)]
    [TestCase(3.0, 1.58496250)]
    [TestCase(5.0, 2.32192809)]
    [TestCase(10.0, 3.32192809)]
    [TestCase(1.25, 0.32192809)]
    [TestCase(1.75, 0.80735492)]
    [TestCase(0.75, -0.41503749)]
    [TestCase(0.9, -0.15200309)]
    [TestCase(32.0, 5.0)]
    [TestCase(64.0, 6.0)]
    [TestCase(128.0, 7.0)]
    public void CalcLog2(double value, double expected)
    {
        var actual = Fix.Log2(value);
        Fix expectedFix = expected;
        actual.Should().BeApproximately(expectedFix);
    }

    [TestCase(0.0, 1.0)]
    [TestCase(1.0, 2.0)]
    [TestCase(2.0, 4.0)]
    [TestCase(3.0, 8.0)]
    [TestCase(4.0, 16.0)]
    [TestCase(-1.0, 0.5)]
    [TestCase(-2.0, 0.25)]
    [TestCase(-3.0, 0.125)]
    [TestCase(0.5, 1.41421356)]
    [TestCase(1.5, 2.82842712)]
    [TestCase(2.5, 5.65685424)]
    [TestCase(0.25, 1.18920711)]
    [TestCase(0.75, 1.68179283)]
    [TestCase(5.5, 45.25483399)]
    [TestCase(6.25, 76.10925536)]
    [TestCase(-0.5, 0.70710678)]
    [TestCase(-1.5, 0.35355339)]
    [TestCase(10.0, 1024.0)]
    public void CalcExp2(double value, double expected)
    {
        var actual = Fix.Exp2(value);
        Fix expectedFix = expected;
        actual.Should().BeApproximately(expectedFix, 0.0011);
    }
}
