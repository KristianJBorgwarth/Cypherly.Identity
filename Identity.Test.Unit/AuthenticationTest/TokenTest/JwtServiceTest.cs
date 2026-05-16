using System.Security.Claims;
using FluentAssertions;
using Identity.Domain.Aggregates;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Identity.Application.Services;
using Identity.Application.Settings;
using FakeItEasy;

namespace Identity.Test.Unit.AuthenticationTest.TokenTest;

public class JwtServiceTest
{
    private readonly JwtService _jwtService;
    private readonly IJwkCache _jwkCache = A.Fake<IJwkCache>();

    public JwtServiceTest()
    {
        var jwtSettings = Options.Create(new JwtSettings
        {
            Secret = "superduperextremetesterinosecretirnosecret_",
            Issuer = "testissuer",
            Audience = "testaudience",
            TokenLifeTimeInMinutes = 10,
        });

        _jwtService = new JwtService(_jwkCache, jwtSettings);
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
        decodedToken.Claims.First(c => c.Type == "device_id").Value.Should().Be(device.Id.ToString());
        decodedToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value.Should().Be(user.Id.ToString());
        decodedToken.Claims.First(c => c.Type == "jti").Value.Should().NotBeNullOrEmpty();
    }
}
