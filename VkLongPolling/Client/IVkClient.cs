using VkLongPolling.Models;

namespace VkLongPolling.Client;

internal interface IVkClient
{
    Task<SessionInfo> GetLongPollSessionAsync();
    Task<LongPollResponse?> GetUpdatesAsync(SessionInfo sessionInfo, CancellationToken cancellationToken);
}