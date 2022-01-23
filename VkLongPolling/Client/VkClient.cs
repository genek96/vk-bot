using System.Text.Json;
using VkLongPolling.Configuration;
using VkLongPolling.Models;

namespace VkLongPolling.Client;

internal class VkClient : IDisposable, IVkClient
{
    public VkClient(ClientSettings clientSettings)
    {
        this.clientSettings = clientSettings;
        SocketsHttpHandler handler = new() { ConnectTimeout = TimeSpan.FromSeconds(90) };
        _client = new(handler);
    }

    public async Task<SessionInfo> GetLongPollSessionAsync()
    {
        var (url, httpContent) = new RequestBuilder(clientSettings.ServerAddress)
            .AddPath("groups.getLongPollServer")
            .WithFormDataContent(
                ("group_id", clientSettings.GroupId),
                ("access_token", clientSettings.Token),
                ("v", clientSettings.ApiVersion)
            )
            .Build();

        var response = await _client.PostAsync(url, httpContent);
        var serverResponse = await ProcessResponseAsync<ServerResponse<SessionInfo>>(response);
        if (serverResponse == null || serverResponse.Response == null)
            throw new HttpRequestException(serverResponse?.Error?.ErrorMsg ?? "Failed to get session info");
        return serverResponse.Response;
    }

    public async Task<LongPollResponse?> GetUpdatesAsync(SessionInfo sessionInfo, CancellationToken cancellationToken)
    {
        var request = new RequestBuilder(sessionInfo.Server)
            .AddQueryParam("act", "a_check")
            .AddQueryParam("key", sessionInfo.Key)
            .AddQueryParam("ts", sessionInfo.Ts)
            .AddQueryParam("wait", clientSettings.WaitTimeout.ToString())
            .Build();

        var response = await _client.GetAsync(request.Url, cancellationToken);
        return await ProcessResponseAsync<LongPollResponse>(response);
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    private static async Task<T?> ProcessResponseAsync<T>(HttpResponseMessage? responseMessage) where T : class
    {
        if (responseMessage == null)
            throw new HttpRequestException("No response received");
        if (!responseMessage.IsSuccessStatusCode)
            throw new HttpRequestException("Unsuccessful status code", null, responseMessage.StatusCode);

        var response = await JsonSerializer.DeserializeAsync<T>(
            await responseMessage.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
                Converters = { new UpdateEventConverter() }
            }
        );

        return response;
    }

    private readonly ClientSettings clientSettings;
    private readonly HttpClient _client;
}