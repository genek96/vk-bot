using VkLongPolling.Models;

namespace VkLongPolling;

internal class RequestBuilder
{
    public RequestBuilder(string baseAddress)
    {
        _uriBuilder = new UriBuilder(baseAddress);
        _queryParams = new Dictionary<string, string>();
    }

    public RequestBuilder AddPath(string path)
    {
        _uriBuilder.Path += "/" + path.Replace("/", "");
        return this;
    }

    public RequestBuilder AddQueryParam(string name, string value)
    {
        _queryParams[name] = value;
        return this;
    }

    public RequestBuilder WithFormDataContent(params (string name, string? value)[] formData)
    {
        MultipartFormDataContent content = new ();
        foreach (var (name, value) in formData)
        {
            if(value != null)
                content.Add(new StringContent(value), name);
        }

        _content = content;
        return this;
    }

    public Request Build()
    {
        var queryParams = string.Empty;
        var hasParams = false;
        foreach (var (name, value) in _queryParams)
        {
            queryParams += hasParams ? $"&{name}={value}" : $"?{name}={value}";
            hasParams = true;
        }

        _uriBuilder.Query = queryParams;
        return new Request(_uriBuilder.ToString(), _content);
    }

    private HttpContent? _content;
    private readonly Dictionary<string, string> _queryParams;
    private readonly UriBuilder _uriBuilder;
}