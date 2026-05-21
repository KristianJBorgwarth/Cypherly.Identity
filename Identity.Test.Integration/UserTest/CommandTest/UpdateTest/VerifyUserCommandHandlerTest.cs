using FluentAssertions;
using Identity.Application.Contracts.Repository;
using Identity.Application.Features.User.Commands.Update.Verify;
using Identity.Domain.Aggregates;
using Identity.Domain.Enums;
using Identity.Domain.ValueObjects;
using Identity.Infrastructure.Persistence.Context;
using Identity.Test.Integration.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Identity.Test.Integration.UserTest.CommandTest.UpdateTest;

public class VerifyUserCommandHandlerTest : IntegrationTestBase
{
    private readonly VerifyUserCommandHandler _sut;
    public VerifyUserCommandHandlerTest(IntegrationTestFactory<Program, IdentityDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<VerifyUserCommandHandler>>();
        _sut = new(logger, userRepository, unitOfWork);
    }

    [Fact]
    public async void Handle_Given_Valid_Command_Should_Verify_And_Return_Success()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@email.dk"), Password.Create("lolwortks?293K"), false);
        user.AddVerificationCode(UserVerificationCodeType.EmailVerification);
        await Db.User.AddAsync(user);
        await Db.SaveChangesAsync();

        var command = new VerifyUserCommand()
        {
            UserId = user.Id,
            VerificationCode = user.GetActiveVerificationCode(UserVerificationCodeType.EmailVerification)!.Code.Value
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        Db.User.AsNoTracking().FirstOrDefault(u => u.Id == user.Id)!.IsVerified.Should().BeTrue();
    }

    [Fact]
    public async void Handle_Invalid_Id_Should_Return_NotFound()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@email.dk"), Password.Create("lolwortks?293K"), false);
        user.AddVerificationCode(UserVerificationCodeType.EmailVerification);
        await Db.User.AddAsync(user);
        await Db.SaveChangesAsync();

        var command = new VerifyUserCommand()
        {
            UserId = Guid.NewGuid(),
            VerificationCode = user.GetActiveVerificationCode(UserVerificationCodeType.EmailVerification)!.Code.Value
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Code.Should().Be("entity.not.found");
        Db.User.AsNoTracking().FirstOrDefault(u => u.Id == user.Id)!.IsVerified.Should().BeFalse();
    }

    [Fact]
    public async void Handle_Invalid_VerificationCode_Should_Return_Fail()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@email.dk"), Password.Create("lolwortks?293K"), false);
        user.AddVerificationCode(UserVerificationCodeType.EmailVerification);
        await Db.User.AddAsync(user);
        await Db.SaveChangesAsync();

        var command = new VerifyUserCommand()
        {
            UserId = user.Id,
            VerificationCode = "InvalidCode"
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error!.Description.Should().Contain("Invalid verification code");
        Db.User.AsNoTracking().FirstOrDefault(u => u.Id == user.Id)!.IsVerified.Should().BeFalse();
    }
}