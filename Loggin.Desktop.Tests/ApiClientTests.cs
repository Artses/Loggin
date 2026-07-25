using System.Net;
using System.Net.Http;
using System.Text;
using Loggin.Desktop.Services;
using Xunit;

namespace Loggin.Desktop.Tests;

public sealed class ApiClientTests
{
    [Fact]
    public void Configure_RejectsNonHttpUrl()
    {
        using var client = new ApiClient(new StubHandler(_ => Json(HttpStatusCode.OK, "{}")));
        Assert.Throws<ArgumentException>(() => client.Configure(new AppSettings { BackendUrl = "file:///tmp/api" }));
    }

    [Fact]
    public async Task Login_UsesExistingBackendContractWithoutBearer()
    {
        HttpRequestMessage? captured = null;
        string? body = null;
        using var client = NewClient(async request =>
        {
            captured = request;
            body = await request.Content!.ReadAsStringAsync();
            return Json(HttpStatusCode.OK,
                """{"token":"jwt","name":"Ana","email":"ana@example.com","role":"User"}""");
        });

        var result = await client.LoginAsync("ana@example.com", "secret", CancellationToken.None);

        Assert.Equal("jwt", result.Token);
        Assert.Equal("/api/Auth/login", captured!.RequestUri!.AbsolutePath);
        Assert.Null(captured.Headers.Authorization);
        Assert.Contains("\"email\":\"ana@example.com\"", body);
    }

    [Fact]
    public async Task GetCollectors_SendsBearerAndDeserializesCaseInsensitive()
    {
        HttpRequestMessage? captured = null;
        using var client = NewClient(request =>
        {
            captured = request;
            return Task.FromResult(Json(HttpStatusCode.OK,
                """[{"Id":"0508d440-7c7e-4c29-b51b-b55986d3d2b8","Name":"API","Url":"localhost:8080"}]"""));
        });
        client.Token = "test-token";

        var collectors = await client.GetCollectorsAsync(CancellationToken.None);

        Assert.Single(collectors);
        Assert.Equal("API", collectors[0].Name);
        Assert.Equal("Bearer", captured!.Headers.Authorization!.Scheme);
        Assert.Equal("test-token", captured.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task ApiError_ExposesBackendMessageAndStatus()
    {
        using var client = NewClient(_ => Task.FromResult(
            Json(HttpStatusCode.NotFound, """{"message":"Log not found"}""")));

        var error = await Assert.ThrowsAsync<ApiException>(
            () => client.FetchLogAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(HttpStatusCode.NotFound, error.StatusCode);
        Assert.Equal("Log not found", error.Message);
    }

    [Theory]
    [InlineData("#6D5EF5", true)]
    [InlineData("not-a-color", false)]
    public void ThemeColorValidation_IsPredictable(string value, bool expected) =>
        Assert.Equal(expected, ThemeService.IsValidColor(value));

    private static ApiClient NewClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
    {
        var client = new ApiClient(new StubHandler(responder));
        client.Configure(new AppSettings { BackendUrl = "https://localhost:7001/" });
        return client;
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string json) => new(status)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private sealed class StubHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder) : HttpMessageHandler
    {
        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
            : this(request => Task.FromResult(responder(request))) { }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            responder(request);
    }
}
