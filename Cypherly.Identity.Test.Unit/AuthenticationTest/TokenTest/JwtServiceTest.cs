using System.Security.Claims;
using Cypherly.Identity.Application.Features.Authentication.Token;
using Cypherly.Identity.Domain.Aggregates;
using Cypherly.Identity.Domain.Entities;
using Cypherly.Identity.Domain.Enums;
using Cypherly.Identity.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Cypherly.Authentication.Test.Unit.AuthenticationTest.TokenTest;

public class JwtServiceTest
{
    private readonly JwtService _jwtService;

    public JwtServiceTest()
    {
        var jwtSettings = Options.Create(new JwtSettings
        {
            Secret = "superduperextremetesterinosecretirnosecret_",
            Issuer = "testissuer",
            Audience = "testaudience",
            TokenLifeTimeInMinutes = 10,
        });

        _jwtService = new JwtService(jwtSettings);
    }

    [Fact]
    public async void GenerateToken_Should_Return_Token_With_Valid_Ids()
    {
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("test@password?23K"), true);

        var device = new Device(Guid.NewGuid(), "testKey", "1.0", DeviceType.Desktop, DevicePlatform.Windows, user.Id);

        user.AddDevice(device);

        // Act
        var token = _jwtService.GenerateToken(user.Id, device.Id);

        // Decode the token
        var jwtHandler = new JsonWebTokenHandler();
        var decodedToken = jwtHandler.ReadJsonWebToken(token);

        // Assert: Verify token claims
        decodedToken.Claims.First(c => c.Type == "sub").Value.Should().Be(device.Id.ToString());
        decodedToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value.Should().Be(user.Id.ToString());
        decodedToken.Claims.First(c => c.Type == "jti").Value.Should().NotBeNullOrEmpty();
    }
}