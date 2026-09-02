using System.Text.Json;

namespace FixN.Core.Tests.Data;

public class PercTests
{
    [Test]
    public void ShouldSerializeJson()
    {
        var sut = (Perc)10;
        var json = JsonSerializer.Serialize(sut);
        json.Should().Be("10");
    }

    [Test]
    public void ShouldDeserializeJson()
    {
        const string json = "10";
        var expected = (Perc)10;
        JsonSerializer.Deserialize<Perc>(json).Should().Be(expected);
    }

    [Test]
    public void ShouldStackPercentageWhenMultiplied()
    {
        var value = 2000;
        var p1 = (Perc)12;
        var p2 = (Perc)50;
        var p = p1 * p2;
        var result = p * value;
        var expected = 120;
        result.Should().Be(expected);
    }
}
