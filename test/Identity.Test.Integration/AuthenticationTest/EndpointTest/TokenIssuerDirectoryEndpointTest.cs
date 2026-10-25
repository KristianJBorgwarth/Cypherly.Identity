using System.Buffers.Text;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using Identity.Application.Features.Authentication.Queries.GetTokenIssuerDirectory;
using Identity.Infrastructure.Persistence.Context;
using Identity.Test.Integration.Setup;
using Microsoft.EntityFrameworkCore;
using PrivacyPass.Keys;

namespace Identity.Test.Integration.AuthenticationTest.EndpointTest;

public class TokenIssuerDirectoryEndpointTest(IntegrationTestFactory<Program, IdentityDbContext> factory) : IntegrationTestBase(factory)
{
    private const string Path = "/.well-known/private-token-issuer-directory";

    private readonly HttpClient _client = factory.CreateClient();

    private async Task<TokenIssuerDirectoryResponse> GetDirectoryWithKeyAsync()
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);

        while (DateTime.UtcNow < deadline)
        {
            var response = await _client.GetAsync(Path);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var directory = JsonSerializer.Deserialize<TokenIssuerDirectoryResponse>(await response.Content.ReadAsStringAsync())!;
            if (directory.TokenKeys.Count > 0) return directory;

            await Task.Delay(250);
        }

        throw new TimeoutException("The token issuer key job did not create a key in time.");
    }

    [Fact]
    public async Task GetDirectory_Should_Use_The_Privacy_Pass_Media_Type()
    {
        // Act
        var response = await _client.GetAsync(Path);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/private-token-issuer-directory");
    }

    [Fact]
    public async Task GetDirectory_Should_Publish_The_Key_The_Job_Created()
    {
        // Act
        var directory = await GetDirectoryWithKeyAsync();

        // Assert
        var stored = await Db.TokenIssuerKey.AsNoTracking().SingleAsync(k => k.IsCurrent);
        directory.IssuerRequestUri.Should().Be("/api/authentication/verify-nonce");
        directory.TokenKeys.Should().ContainSingle();
        directory.TokenKeys[0].TokenType.Should().Be(2);
        Base64Url.DecodeFromChars(directory.TokenKeys[0].TokenKey).Should().Equal(stored.PublicKey);
    }

    [Fact]
    public async Task GetDirectory_Should_Publish_A_Key_The_Origin_Can_Read()
    {
        // Act
        var directory = await GetDirectoryWithKeyAsync();

        // Assert
        using var publicKey = TokenKeyId.DecodePublicKey(Base64Url.DecodeFromChars(directory.TokenKeys[0].TokenKey));
        publicKey.KeySize.Should().Be(2048);
    }
}
