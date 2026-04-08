using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HostingQr.Api.Tests;

public sealed class PingEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PingEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPing_ReturnsOkWithServiceName()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/ping");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PingResponse? payload = await response.Content.ReadFromJsonAsync<PingResponse>();

        Assert.NotNull(payload);
        Assert.Equal("HostingQr.Api", payload.Name);
        Assert.False(string.IsNullOrWhiteSpace(payload.Environment));
    }

    public sealed record PingResponse(string Name, string Environment, DateTimeOffset UtcTime);
}
