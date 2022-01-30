using VkLongPolling.Models;

namespace VkLongPolling.Client;

public interface IResponder
{
    Task<CallbackAnswerResponse?> SendMessageEventAnswerAsync(int userId, string eventId, IMessageEventAnswer answer);
    Task<SendMessageResponse> SendMessageAsync(int userId, string message, KeyboardBuilder? keyboardBuilder);
}