namespace VkLongPolling.Configuration;

public class ClientSettings
{
    public string ServerAddress { get; init; }
    public string GroupId { get; init; }
    public string Token { get; init; }
    public string? ApiVersion { get; init; }
    public int WaitTimeout { get; init; }
}