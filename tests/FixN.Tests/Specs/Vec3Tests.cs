using System.Text.Json;

namespace FixN.Core.Tests.Data;

public class Vec3Tests
{
    [Test]
    public void ShouldSerializeJson()
    {
        Vec3 sut = new(111, 555, 999);
        var json = JsonSerializer.Serialize(sut);
        json.Should().Be("[111,555,999]");
    }

    [Test]
    public void ShouldDeserializeJson()
    {
        const string json = "[111,555,999]";
        Vec3 expected = new(111, 555, 999);
        JsonSerializer.Deserialize<Vec3>(json).Should().Be(expected);
    }
}
