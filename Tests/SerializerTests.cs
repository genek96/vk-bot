using System.Text.Json;
using NUnit.Framework;
using VkLongPolling.Client;
using VkLongPolling.Models;

namespace Tests;

public class SerializerTests
{
    [Test]
    public void Serrializer_WithCustomOption_ShouldWorkCorrect()
    {
        var serrialized =
            "{\"type\": \"group_join\",\"object\": {\"user_id\": 1,\"join_type\": \"approved\"},\"group_id\": 1}";
        var result = JsonSerializer.Deserialize<UpdateEvent>(
            serrialized,
            new JsonSerializerOptions()
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
                Converters = { new UpdateEventConverter() }
            }
        );

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Object, Is.AssignableTo(typeof(JoinGroupEvent)));

        var joinEvent = result.Object as JoinGroupEvent;
        Assert.That(joinEvent.JoinType, Is.EqualTo("approved"));
        Assert.That(joinEvent.UserId, Is.EqualTo(1));
    }
}