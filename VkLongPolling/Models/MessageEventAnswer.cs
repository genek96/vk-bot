using System.Text.Json.Serialization;
using VkLongPolling.Client.Serialization;

namespace VkLongPolling.Models;

[JsonConverter(typeof(MessageEventAnswerConverter))]
public interface IMessageEventAnswer
{
    string Type { get; }
}

public record SnackbarAnswer(string Text) : IMessageEventAnswer
{
    public string Type => "show_snackbar";
}

public record OpenLinkAnswer(string Link) : IMessageEventAnswer
{
    public string Type => "open_link";
}

public record CallbackAnswerResponse(int Response);