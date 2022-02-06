using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using VkLongPolling.Models;

namespace VkLongPolling.Client.Serialization;

public class MessageConverter: JsonConverter<Message>
{
    public override Message? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jObject = JsonNode.Parse(ref reader);
        if (jObject == null)
            return null;

        var id = int.Parse(jObject["id"]!.ToString());
        var date = int.Parse(jObject["date"]!.ToString());
        var peerId = int.Parse(jObject["peer_id"]!.ToString());
        var fromId = int.Parse(jObject["from_id"]!.ToString());
        var randomId = jObject["random_id"].Deserialize<int?>();
        var text = jObject["text"]!.ToString();
        var strPayload = jObject["payload"]?.ToString();
        var payload = strPayload != null
            ? JsonSerializer.Deserialize<Payload?>(strPayload, options)
            : null;

        return new Message(id, date, peerId, fromId, text, randomId, payload);
    }

    public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}