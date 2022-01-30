using VkLongPolling.Models;

namespace VkLongPolling.Client;

internal interface IVkClient: IDisposable
{
    Task<SessionInfo> GetLongPollSessionAsync();
    Task<LongPollResponse?> GetUpdatesAsync(SessionInfo sessionInfo, CancellationToken cancellationToken);
    Task<SendMessageResponse> SendMessageAsync(int userId, string message, Keyboard? keyboard);
    Task<CallbackAnswerResponse?> SendMessageEventAnswerAsync(int userId, string eventId, IMessageEventAnswer answer);
}