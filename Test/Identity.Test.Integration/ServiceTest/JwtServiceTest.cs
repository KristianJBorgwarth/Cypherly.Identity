using System.Security.Claims;
using FluentAssertions;
using Identity.Application.Interfaces;
using Identity.Infrastructure.Persistence.Context;
using Identity.Test.Integration.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Test.Integration.ServiceTest;

public class JwtServiceTest(IntegrationTestFactory<Program, IdentityDbContext> factory) : IntegrationTestBase(factory)
{
    private readonly IJwtService _jwtService = factory.Services.CreateScope().ServiceProvider.GetRequiredService<IJwtService>();

    [Fact]
    public async Task GenerateTokenAsync_Should_Return_Valid_Jwt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();

        // Act
        var token = await _jwtService.GenerateTokenAsync(userId, deviceId);

        // Assert
        token.Should().NotBeNullOrEmpty();
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Should().NotBeNull();
        jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value.Should().Be(userId.ToString());
        jwtToken.Claims.FirstOrDefault(c => c.Type == "device_id")?.Value.Should().Be(deviceId.ToString());
    }

}
