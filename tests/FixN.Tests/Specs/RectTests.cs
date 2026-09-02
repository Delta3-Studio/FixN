using System.Text.Json;

namespace FixN.Core.Tests.Data;

public class RectTests
{
    [Test]
    public void ShouldSerializeJson()
    {
        Rect sut = new(111, 222, 888, 999);
        var json = JsonSerializer.Serialize(sut);
        json.Should().Be("[111,222,888,999]");
    }

    [Test]
    public void ShouldDeserializeJson()
    {
        const string json = "[111,222,888,999]";
        Rect expected = new(111, 222, 888, 999);
        JsonSerializer.Deserialize<Rect>(json).Should().Be(expected);
    }
}
