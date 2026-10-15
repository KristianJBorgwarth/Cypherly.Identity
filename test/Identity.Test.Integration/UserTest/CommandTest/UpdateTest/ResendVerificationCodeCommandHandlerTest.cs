using FluentAssertions;
using Identity.Application.Caching.LoginNonce;
using Identity.Application.Contracts.Cache;
using Identity.Application.Contracts.RateLimiting;
using Identity.Application.Contracts.Repository;
using Identity.Application.Features.User.Commands.Update.ResendVerificationCode;
using Identity.Domain.Aggregates;
using Identity.Domain.Common;
using Identity.Domain.Enums;
using Identity.Domain.Services.User;
using Identity.Domain.ValueObjects;
using Identity.Infrastructure.Persistence.Context;
using Identity.Test.Integration.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable EntityFramework.NPlusOne.IncompleteDataUsage

namespace Identity.Test.Integration.UserTest.CommandTest.UpdateTest;

public class ResendVerificationCodeCommandHandlerTest : IntegrationTestBase
{
    private readonly ResendVerificationCodeCommandHandler _sut;
    private readonly ILoginNonceCache _loginNonceCache;
    public ResendVerificationCodeCommandHandlerTest(IntegrationTestFactory<Program, IdentityDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var verificationCodeService = scope.ServiceProvider.GetRequiredService<IVerificationCodeService>();
        var rateLimiter = scope.ServiceProvider.GetRequiredService<IRateLimiter>();
        _loginNonceCache = scope.ServiceProvider.GetRequiredService<ILoginNonceCache>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ResendVerificationCodeCommandHandler>>();
        _sut = new(repo, unitOfWork, verificationCodeService, rateLimiter, _loginNonceCache, logger);
    }

    [Fact]
    public async Task Handle_Valid_Command_Should_Generate_New_Verification_Code()
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
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        Db.User.AsNoTracking().Include(user => user.VerificationCodes).First().VerificationCodes.Should().HaveCount(1);
        Db.User.AsNoTracking().Include(user => user.VerificationCodes).First().VerificationCodes.First().CodeType.Should().Be(UserVerificationCodeType.EmailVerification);
    }

    [Fact]
    public async Task Handle_Login_With_Password_Nonce_Should_Generate_New_Code_And_Burn_Old()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("kj9823HHj?"), true);
        user.AddVerificationCode(UserVerificationCodeType.Login);

        await Db.User.AddAsync(user);
        await Db.SaveChangesAsync();

        var passwordNonce = LoginNonce.Create(user.Id, LoginNonceStage.PasswordVerified);
        await _loginNonceCache.AddNonceAsync(passwordNonce, CancellationToken.None);

        var cmd = new ResendVerificationCodeCommand
        {
            UserId = user.Id,
            CodeType = UserVerificationCodeType.Login,
            NonceId = passwordNonce.Id,
            Nonce = passwordNonce.NonceValue,
        };

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var codes = Db.User.AsNoTracking().Include(u => u.VerificationCodes).First().VerificationCodes;
        codes.Should().HaveCount(2);
        codes.Should().ContainSingle(vc => !vc.Code.IsUsed);
        (await _loginNonceCache.GetNonceAsync(passwordNonce.Id, CancellationToken.None)).Should().NotBeNull();
        await _loginNonceCache.DeteleNonceAsync(passwordNonce.Id, CancellationToken.None);
    }

    [Fact]
    public async Task Handle_Login_Without_Nonce_Should_Return_Unauthorized()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("kj9823HHj?"), true);

        await Db.User.AddAsync(user);
        await Db.SaveChangesAsync();

        var cmd = new ResendVerificationCodeCommand
        {
            UserId = user.Id,
            CodeType = UserVerificationCodeType.Login,
        };

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Errors.General.Unauthorized());
        Db.User.AsNoTracking().Include(u => u.VerificationCodes).First().VerificationCodes.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Given_Too_Many_Resends_Should_Return_TooManyRequests()
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

        for (var i = 0; i < 5; i++)
            (await _sut.Handle(cmd, CancellationToken.None)).Success.Should().BeTrue();

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Errors.General.TooManyRequests());
        Db.User.AsNoTracking().Include(u => u.VerificationCodes).First().VerificationCodes.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_Command_With_Invalid_Id_Should_Return_Result_Fail()
    {
        // Arrange
        var cmd = new ResendVerificationCodeCommand
        {
            UserId = Guid.NewGuid(),
            CodeType = UserVerificationCodeType.EmailVerification,
        };

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Code.Should().Be("entity.not.found");
    }

    [Fact]
    public async Task Handle_User_With_Verified_Status_Should_Return_Result_Fail()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("kj9823HHj?"),
            true); // Verified status set to true

        await Db.User.AddAsync(user);
        await Db.SaveChangesAsync();

        var cmd = new ResendVerificationCodeCommand
        {
            UserId = user.Id,
            CodeType = UserVerificationCodeType.EmailVerification,
        };

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Description.Should().Be("User is already verified");
    }
}