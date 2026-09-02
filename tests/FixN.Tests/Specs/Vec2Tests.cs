using System.Text.Json;

namespace FixN.Core.Tests.Data;

public class Vec2Tests
{
    [Test]
    public void ShouldSerializeJson()
    {
        Vec2 sut = new(111, 999);
        var json = JsonSerializer.Serialize(sut);
        json.Should().Be("[111,999]");
    }

    [Test]
    public void ShouldDeserializeJson()
    {
        const string json = "[111,999]";
        Vec2 expected = new(111, 999);
        JsonSerializer.Deserialize<Vec2>(json).Should().Be(expected);
    }

    public class RotatorTests
    {
        [Test]
        public void ShouldSerializeJson()
        {
            var sut = Vec2.Rotator.FromDegrees(90);
            var json = JsonSerializer.Serialize(sut);
            json.Should().StartWith("90.");
        }

        [Test]
        public void ShouldDeserializeJson()
        {
            const string json = "90";
            var sut = JsonSerializer.Deserialize<Vec2.Rotator>(json);
            sut.Degrees.Should().BeApproximately(90, 0.1);
        }

        [Test]
        public void ShouldSerializeHighJson()
        {
            var sut = Vec2.Rotator.FromDegrees(180);
            var json = JsonSerializer.Serialize(sut);
            json.Should().StartWith("180.");
        }
    }
}
