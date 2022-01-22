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

    public async Task<SessionInfo> GetLongPollServerAsync()
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
        return (await ProcessResponseAsync<SessionInfo>(response))!;
    }

    public async Task<LongPollResponse?> GetUpdatesAsync(SessionInfo sessionInfo, CancellationToken cancellationToken)
    {
        var request = new RequestBuilder(sessionInfo.Server)
            .AddQueryParam("act", "a_check")
            .AddQueryParam("key", sessionInfo.Key)
            .AddQueryParam("ts", sessionInfo.Ts)
            .AddQueryParam("wait", clientSettings.waitTimeout.ToString())
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

        var response = await JsonSerializer.DeserializeAsync<ServerResponse<T>>(
            await responseMessage.Content.ReadAsStreamAsync());

        if (response == null)
            return null;
        if (response.Response == null && response.Error != null)
            throw new HttpRequestException(response.Error.ErrorMsg);

        return response.Response;
    }

    private readonly ClientSettings clientSettings;
    private readonly HttpClient _client;
}