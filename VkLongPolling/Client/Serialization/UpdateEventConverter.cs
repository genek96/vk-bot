using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using VkLongPolling.Models;

namespace VkLongPolling.Client.Serialization;

public class UpdateEventConverter: JsonConverter<UpdateEvent>
{
    public override UpdateEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jObject = JsonNode.Parse(ref reader);
        if (jObject == null)
            return null;

        var type = jObject["type"]!.ToString();
        var groupId = jObject["group_id"]!.ToString();
        var updateJObject = jObject["object"]!;

        IUpdateEventObject updateEventObject = type switch
        {
            "message_new" => updateJObject.Deserialize<NewMessageEvent>(options)!,
            "group_join" => updateJObject.Deserialize<JoinGroupEvent>(options)!,
            "message_event" => updateJObject.Deserialize<MessageEvent>(options)!,
            _ => new UnknownEvent(updateJObject.ToString())
        };

        return new UpdateEvent(type, updateEventObject, groupId);
    }

    public override void Write(Utf8JsonWriter writer, UpdateEvent value, JsonSerializerOptions options)
    {
        throw new NotImplementedException("This is read only converter");
    }
}