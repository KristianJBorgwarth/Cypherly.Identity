using Cypherly.Identity.Domain.Aggregates;
using Cypherly.Identity.Domain.Enums;
using Cypherly.Identity.Domain.Events.User;
using Cypherly.Identity.Domain.Services.User;
using Cypherly.Identity.Domain.ValueObjects;
using FluentAssertions;

namespace Cypherly.Authentication.Test.Unit.ServiceTest;

public class VerificationCodeServiceTest
{
    private readonly IVerificationCodeService _sut = new VerificationCodeService();

    [Theory]
    [InlineData(UserVerificationCodeType.EmailVerification)]
    [InlineData(UserVerificationCodeType.PasswordReset)]
    public void GenerateVerificationCode_Should_Add_VerificationCode_And_DomainEvent(UserVerificationCodeType codeType)
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("kjshsdi9?A"), false);

        // Act
        _sut.GenerateVerificationCode(user, codeType);

        // Assert
        user.VerificationCodes.Should().HaveCount(1);
        user.VerificationCodes.First().CodeType.Should().Be(codeType);
        user.DomainEvents.Should().ContainSingle(e => e is VerificationCodeGeneratedEvent);
    }

}