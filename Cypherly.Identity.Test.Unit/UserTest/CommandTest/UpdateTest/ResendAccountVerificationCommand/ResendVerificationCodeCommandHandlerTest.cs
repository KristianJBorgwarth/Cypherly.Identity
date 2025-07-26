using Cypherly.Identity.Application.Contracts;
using Cypherly.Identity.Application.Contracts.Repository;
using Cypherly.Identity.Application.Features.User.Commands.Update.ResendVerificationCode;
using Cypherly.Identity.Domain.Aggregates;
using Cypherly.Identity.Domain.Enums;
using Cypherly.Identity.Domain.Services.User;
using Cypherly.Identity.Domain.ValueObjects;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Cypherly.Authentication.Test.Unit.UserTest.CommandTest.UpdateTest.ResendAccountVerificationCommand;

public class ResendVerificationCodeCommandHandlerTest
{
    private readonly IUserRepository _fakeRepo;
    private readonly IUnitOfWork _fakeUnitOfWork;
    private readonly IVerificationCodeService _fakeVerificationCodeService;
    private readonly ResendVerificationCodeCommandHandler _sut;

    public ResendVerificationCodeCommandHandlerTest()
    {
        _fakeRepo = A.Fake<IUserRepository>();
        _fakeUnitOfWork = A.Fake<IUnitOfWork>();
        _fakeVerificationCodeService = A.Fake<IVerificationCodeService>();
        var fakeLogger = A.Fake<ILogger<ResendVerificationCodeCommandHandler>>();
        _sut = new ResendVerificationCodeCommandHandler(_fakeRepo, _fakeUnitOfWork, _fakeVerificationCodeService, fakeLogger);
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
        A.CallTo(() => _fakeRepo.GetByIdAsync(cmd.UserId))!.Returns<User>(null);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Code.Should().Be("entity.not.found");
    }

    [Fact]
    public async Task Handle_Command_With_User_Verified_Should_Return_Result_Fail()
    {
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("kjsKidh??923"), true);
        var cmd = new ResendVerificationCodeCommand
        {
            UserId = user.Id,
            CodeType = UserVerificationCodeType.EmailVerification,
        };
        A.CallTo(() => _fakeRepo.GetByIdAsync(cmd.UserId)).Returns(user);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Be("User is already verified");
    }

    [Fact]
    public async Task Handle_Command_When_Something_Throws_Exception_Should_Return_Result_Fail()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("kjsKidh??923"), false);
        var cmd = new ResendVerificationCodeCommand
        {
            UserId = user.Id,
            CodeType = UserVerificationCodeType.EmailVerification,
        };
        A.CallTo(() => _fakeRepo.GetByIdAsync(cmd.UserId)).Returns(user);
        A.CallTo(() => _fakeVerificationCodeService.GenerateVerificationCode(user, UserVerificationCodeType.EmailVerification)).Throws<Exception>();

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Be("An exception occurred while resending verification code for user");
    }

    [Fact]
    public async Task Handle_Command_With_Valid_Id_Should_Generate_New_Verification_Code_And_Return_Result_Ok()
    {

        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("kjsKidh??923"), false);
        var cmd = new ResendVerificationCodeCommand
        {
            UserId = user.Id,
            CodeType = UserVerificationCodeType.EmailVerification,

        };
        A.CallTo(() => _fakeRepo.GetByIdAsync(cmd.UserId)).Returns(user);
        A.CallTo(() => _fakeVerificationCodeService.GenerateVerificationCode(user, UserVerificationCodeType.EmailVerification)).DoesNothing();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(CancellationToken.None)).DoesNothing();

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Success.Should().BeTrue();
        A.CallTo(() => _fakeRepo.GetByIdAsync(cmd.UserId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeVerificationCodeService.GenerateVerificationCode(user, UserVerificationCodeType.EmailVerification)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(CancellationToken.None)).MustHaveHappenedOnceExactly();

    }
}