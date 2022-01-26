using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using VkLongPolling.Client.Serialization;
using VkLongPolling.Configuration;
using VkLongPolling.Models;

namespace VkLongPolling.Client;

internal class VkClient : IVkClient
{
    public VkClient(ClientSettings clientSettings)
    {
        _clientSettings = clientSettings;
        SocketsHttpHandler handler = new() { ConnectTimeout = TimeSpan.FromSeconds(90) };
        _client = new(handler);
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            Converters = { new JsonStringEnumConverter(new SnakeCaseNamingPolicy()) },
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };
    }

    public async Task<SessionInfo> GetLongPollSessionAsync()
    {
        var (url, httpContent) = new RequestBuilder(_clientSettings.ServerAddress)
            .AddPath("groups.getLongPollServer")
            .WithFormDataContent(
                ("group_id", _clientSettings.GroupId),
                ("access_token", _clientSettings.Token),
                ("v", _clientSettings.ApiVersion)
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
            .AddQueryParam("wait", _clientSettings.WaitTimeout.ToString())
            .Build();

        var response = await _client.GetAsync(request.Url, cancellationToken);
        return await ProcessResponseAsync<LongPollResponse>(response);
    }

    public async Task<SendMessageResponse> SendMessageAsync(int userId, string message, Keyboard? keyboard)
    {
        var requestBuilder = new RequestBuilder(_clientSettings.ServerAddress)
            .AddPath("messages.send")
            .AddQueryParam("access_token", _clientSettings.Token)
            .AddQueryParam("v", _clientSettings.ApiVersion)
            .AddQueryParam("user_id", userId.ToString())
            .AddQueryParam("random_id", Random.Shared.Next().ToString())
            .AddQueryParam("peer_id", userId.ToString())
            .AddQueryParam("message", message);

        if (keyboard != null)
            requestBuilder.AddQueryParam("keyboard", JsonSerializer.Serialize(keyboard, _serializerOptions));

        var request = requestBuilder.Build();
        var response = await _client.GetAsync(request.Url);
        return (await ProcessResponseAsync<SendMessageResponse>(response))!;
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    private async Task<T?> ProcessResponseAsync<T>(HttpResponseMessage? responseMessage) where T : class
    {
        if (responseMessage == null)
            throw new HttpRequestException("No response received");
        if (!responseMessage.IsSuccessStatusCode)
            throw new HttpRequestException("Unsuccessful status code", null, responseMessage.StatusCode);

        var str = await responseMessage.Content.ReadAsStringAsync();
        var response = await JsonSerializer.DeserializeAsync<T>(
            await responseMessage.Content.ReadAsStreamAsync(),
            _serializerOptions
        );

        return response;
    }

    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ClientSettings _clientSettings;
    private readonly HttpClient _client;
}