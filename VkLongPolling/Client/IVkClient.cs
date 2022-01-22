using VkLongPolling.Models;

namespace VkLongPolling.Client;

internal interface IVkClient
{
    Task<SessionInfo> GetLongPollServerAsync();
    Task<LongPollResponse?> GetUpdatesAsync(SessionInfo sessionInfo, CancellationToken cancellationToken);
}