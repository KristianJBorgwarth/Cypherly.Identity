using System.Net;
using System.Text.Json;
using FluentAssertions;
using Identity.Application.Features.Authentication.Queries.GetJwks;
using Identity.Infrastructure.Persistence.Context;
using Identity.Test.Integration.Setup;

namespace Identity.Test.Integration.AuthenticationTest.EndpointTest;

public class JwksEndpointTest(IntegrationTestFactory<Program, IdentityDbContext> factory) : IntegrationTestBase(factory)
{
    private readonly HttpClient _client = factory.CreateClient();
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task GetJwks_Should_Return_JwksResponse()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/.well-known/jwks.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        var jwks = JsonSerializer.Deserialize<JwksResponse>(content, _jsonOptions);
        jwks.Should().NotBeNull();
        jwks.Keys.Should().NotBeNullOrEmpty();
        jwks.Keys[0].Kid.Should().NotBeNullOrEmpty();
        jwks.Keys[0].Kty.Should().NotBeNullOrEmpty();
        jwks.Keys[0].Use.Should().NotBeNullOrEmpty();
        jwks.Keys[0].Alg.Should().NotBeNullOrEmpty();
        jwks.Keys[0].N.Should().NotBeNullOrEmpty();
        jwks.Keys[0].E.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetJwks_GivenMultipleRequests_Should_Return_CachedKey()
    {
        // Arrange
        var firstResponse = await _client.GetAsync("/.well-known/jwks.json");
        var firstContent = await firstResponse.Content.ReadAsStringAsync();
        var firstJwks = JsonSerializer.Deserialize<JwksResponse>(firstContent, _jsonOptions);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstJwks.Should().NotBeNull();
        firstJwks.Keys.Should().NotBeNullOrEmpty();

        // Act
        var secondResponse = await _client.GetAsync("/.well-known/jwks.json");
        var secondContent = await secondResponse.Content.ReadAsStringAsync();
        var secondJwks = JsonSerializer.Deserialize<JwksResponse>(secondContent, _jsonOptions);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondJwks.Should().NotBeNull();
        secondJwks.Keys.Should().NotBeNullOrEmpty();
        secondJwks.Keys.Count.Should().Be(firstJwks.Keys.Count);
        secondJwks.Keys[0].Kid.Should().Be(firstJwks.Keys[0].Kid);
        secondJwks.Keys[0].Kty.Should().Be(firstJwks.Keys[0].Kty);
        secondJwks.Keys[0].Use.Should().Be(firstJwks.Keys[0].Use);
        secondJwks.Keys[0].Alg.Should().Be(firstJwks.Keys[0].Alg);
        secondJwks.Keys[0].N.Should().Be(firstJwks.Keys[0].N);
        secondJwks.Keys[0].E.Should().Be(firstJwks.Keys[0].E);
    }
}
