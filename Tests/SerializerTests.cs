using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;
using VkLongPolling.Client.Serialization;
using VkLongPolling.Models;

namespace Tests;

public class SerializerTests
{
    [Test]
    public void Serrializer_WithCustomOption_ShouldDeserializeCorrect()
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

    [Test]
    public void Serializer_ForButtons_ShouldSerializeCorrect()
    {
        var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        var keyboard = new Button(new TextButtonAction("Хоп", new Payload("ла ла ла")), ButtonColor.Primary);

        var serialized = JsonSerializer.Serialize(keyboard, options);
        var result = JsonSerializer.Deserialize<Button>(serialized, options);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Action, Is.InstanceOf<TextButtonAction>());
        var action = result.Action as TextButtonAction;
        Assert.That(action.Label, Is.EqualTo("Хоп"));
    }
}