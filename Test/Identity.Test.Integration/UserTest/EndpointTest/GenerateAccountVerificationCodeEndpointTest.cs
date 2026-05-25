using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Identity.Application.Features.User.Commands.Update.ResendVerificationCode;
using Identity.Domain.Aggregates;
using Identity.Domain.Enums;
using Identity.Domain.ValueObjects;
using Identity.Infrastructure.Persistence.Context;
using Identity.Test.Integration.Setup;
using Microsoft.EntityFrameworkCore;

// ReSharper disable EntityFramework.NPlusOne.IncompleteDataUsage
// ReSharper disable EntityFramework.NPlusOne.IncompleteDataQuery

namespace Identity.Test.Integration.UserTest.EndpointTest;

public class GenerateAccountVerificationCodeEndpointTest(IntegrationTestFactory<Program, IdentityDbContext> factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Given_Valid_Request_Should_Generate_New_Verification_Code_And_Return_200_Ok()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("kj9823HHj?"), false);

        await Db.User.AddAsync(user);
        await Db.SaveChangesAsync();

        var cmd = new ResendVerificationCodeCommand
        {
            UserId = user.Id,
            CodeType = UserVerificationCodeType.EmailVerification,
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/user/resend-verification", cmd);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        Db.User.Include(u => u.VerificationCodes).FirstOrDefault()!.VerificationCodes.Should().HaveCount(1);
    }

    [Fact]
    public async Task Given_Invalid_Request_Should_Return_400_BadRequest()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("kj9823HHj?"), true); // USER IS ALREADY VERIFIED

        await Db.User.AddAsync(user);
        await Db.SaveChangesAsync();

        var cmd = new ResendVerificationCodeCommand
        {
            UserId = user.Id,
            CodeType = UserVerificationCodeType.EmailVerification,
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/user/resend-verification", cmd);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        Db.User.AsNoTracking().First().VerificationCodes.Should().HaveCount(0);
    }

}