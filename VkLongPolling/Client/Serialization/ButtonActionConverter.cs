using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using VkLongPolling.Models;

namespace VkLongPolling.Client.Serialization;

public class ButtonActionConverter: JsonConverter<IButtonAction>
{
    public override IButtonAction? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jObject = JsonNode.Parse(ref reader);
        if (jObject == null)
            return null;

        var type = jObject["Type"]!.ToString();

        return type switch
        {
            "Text" => jObject.Deserialize<TextButtonAction>(options)!,
            "Callback" => jObject.Deserialize<CallbackButtonAction>(options)!,
            _ => throw new ArgumentException("Attempt to parse unknown button action")
        };
    }

    public override void Write(Utf8JsonWriter writer, IButtonAction value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case null:
                JsonSerializer.Serialize(writer, value, options);
                break;
            default:
            {
                var type = value.GetType();
                JsonSerializer.Serialize(writer, value, type, options);
                break;
            }
        }
    }
}