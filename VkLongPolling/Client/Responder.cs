using VkLongPolling.Models;

namespace VkLongPolling.Client;

public class Responder : IResponder
{
    internal Responder(IVkClient vkClient)
    {
        _vkClient = vkClient;
    }

    public Task<CallbackAnswerResponse?> SendMessageEventAnswerAsync(
        int userId,
        string eventId,
        IMessageEventAnswer answer
    )
    {
        return _vkClient.SendMessageEventAnswerAsync(userId, eventId, answer);
    }

    public Task<SendMessageResponse> SendMessageAsync(int userId, string message, KeyboardBuilder? keyboardBuilder)
    {
        var keyboard = keyboardBuilder ?? (Keyboard?)null;
        return _vkClient.SendMessageAsync(userId, message, keyboard);
    }

    private readonly IVkClient _vkClient;
}