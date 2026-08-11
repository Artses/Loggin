using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Loggin.Desktop.Models;

namespace Loggin.Desktop.Services;

public sealed class ApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public string? Token { get; set; }

    public ApiClient(HttpMessageHandler? handler = null)
    {
        _httpClient = handler is null ? new HttpClient() : new HttpClient(handler);
    }

    public void Configure(AppSettings settings)
    {
        if (!Uri.TryCreate(settings.BackendUrl, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("http" or "https"))
            throw new ArgumentException("O endereço do backend deve ser uma URL HTTP ou HTTPS válida.");
        _httpClient.BaseAddress = uri.AbsoluteUri.EndsWith('/') ? uri : new Uri(uri.AbsoluteUri + "/");
        _httpClient.Timeout = TimeSpan.FromSeconds(Math.Clamp(settings.TimeoutSeconds, 3, 300));
    }

    public Task<AuthResponse> LoginAsync(string email, string password, CancellationToken ct) =>
        SendAsync<AuthResponse>(HttpMethod.Post, "api/Auth/login", new { email, password }, ct);

    public Task<AuthResponse> RegisterAsync(string name, string email, string password, CancellationToken ct) =>
        SendAsync<AuthResponse>(HttpMethod.Post, "api/Auth/register", new { name, email, password }, ct);

    public Task VerifyAsync(CancellationToken ct) =>
        SendAsync<object>(HttpMethod.Get, "api/Auth/hi", null, ct);

    public Task<List<CollectorDto>> GetCollectorsAsync(CancellationToken ct) =>
        SendAsync<List<CollectorDto>>(HttpMethod.Get, "api/Collector", null, ct);

    public Task<CollectorDto> AddCollectorAsync(string name, string url, CancellationToken ct) =>
        SendAsync<CollectorDto>(HttpMethod.Post, "api/Collector", new { name, url }, ct);

    public Task<CollectorDto> UpdateCollectorAsync(Guid id, string name, string url, CancellationToken ct) =>
        SendAsync<CollectorDto>(HttpMethod.Put, "api/Collector", new { id, name, url }, ct);

    public Task DeleteCollectorAsync(Guid id, CancellationToken ct) =>
        SendNoContentAsync(HttpMethod.Delete, "api/Collector", new { id }, ct);

    public Task<List<LogDto>> GetLogsAsync(CancellationToken ct) =>
        SendAsync<List<LogDto>>(HttpMethod.Get, "api/Log", null, ct);

    public Task<LogDto> AddLogAsync(Guid collectorId, string path, CancellationToken ct) =>
        SendAsync<LogDto>(HttpMethod.Post, "api/Log", new { collectorId, path }, ct);

    public Task<LogDto> UpdateLogAsync(Guid id, Guid collectorId, string path, CancellationToken ct) =>
        SendAsync<LogDto>(HttpMethod.Put, "api/Log", new { id, collectorId, path }, ct);

    public Task DeleteLogAsync(Guid id, CancellationToken ct) =>
        SendNoContentAsync(HttpMethod.Delete, "api/Log", new { id }, ct);

    public Task<List<LogLineDto>> FetchLogAsync(Guid id, Guid collectorId, CancellationToken ct) =>
        SendAsync<List<LogLineDto>>(HttpMethod.Post, "api/Log/GetLogs", new { id, collectorId }, ct);

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;
        var message = response.ReasonPhrase ?? "Falha na comunicação com o backend.";
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>(JsonOptions, ct);
            if (!string.IsNullOrWhiteSpace(error?.Message))
                message = error.Message;
        }
        catch { }
        throw new ApiException(message, response.StatusCode);
    }

    private async Task SendNoContentAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var response = await SendRequestAsync(method, path, body, ct);
        await EnsureSuccessAsync(response, ct);
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var response = await SendRequestAsync(method, path, body, ct);
        await EnsureSuccessAsync(response, ct);
        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
        return result ?? throw new ApiException("O backend retornou uma resposta vazia ou inválida.", response.StatusCode);
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);
        if (!string.IsNullOrWhiteSpace(Token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        if (body is not null)
            request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
    }

    public void Dispose() => _httpClient.Dispose();
}

public sealed class ApiException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
