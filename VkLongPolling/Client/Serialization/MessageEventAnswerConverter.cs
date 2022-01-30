using System.Text.Json;
using System.Text.Json.Serialization;
using VkLongPolling.Models;

namespace VkLongPolling.Client.Serialization;

public class MessageEventAnswerConverter: JsonConverter<IMessageEventAnswer>
{
    public override IMessageEventAnswer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException($"{nameof(MessageEventAnswerConverter)} is write only");
    }

    public override void Write(Utf8JsonWriter writer, IMessageEventAnswer? value, JsonSerializerOptions? options)
    {
        if (value == null)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
        else
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}