using System.Text.Json.Serialization;
using VkLongPolling.Client.Serialization;

namespace VkLongPolling.Models;

public record SendMessageResponse(int PeerId, long MessageId, long ConversationMessageId, string Error);

public record Keyboard(bool OneTime, Button[][] Buttons, bool Inline);

[JsonConverter(typeof(ButtonActionConverter))]
public interface IButtonAction { ButtonActionType Type { get; } }

public record Button(IButtonAction Action, ButtonColor Color);

public record TextButtonAction(string Label, Payload? Payload) : IButtonAction
{
    public ButtonActionType Type => ButtonActionType.Text;
}

public record CallbackButtonAction(string Label, Payload? Payload) : IButtonAction
{
    public ButtonActionType Type => ButtonActionType.Callback;
}

public record Payload(string Text);

public enum ButtonColor { Primary, Secondary, Negative, Positive }

public enum ButtonActionType { Text, Callback }
