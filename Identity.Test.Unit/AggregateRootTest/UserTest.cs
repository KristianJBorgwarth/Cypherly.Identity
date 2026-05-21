using FluentAssertions;
using Identity.Domain.Aggregates;
using Identity.Domain.Enums;
using Identity.Domain.ValueObjects;

namespace Identity.Test.Unit.AggregateRootTest;

public class UserTest
{
    [Fact]
    public void AddVerificationCode_Should_Add_New_VerificationCode()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("kjFis97823??sd"), false);

        // Act
        user.AddVerificationCode(UserVerificationCodeType.EmailVerification);

        // Assert
        user.VerificationCodes.Should().HaveCount(1);
    }

    [Fact]
    public void AddVerificationCode_Should_Mark_Existing_Codes_Of_Same_Type_As_Used()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("kjFis97823??sd"), false);
        user.AddVerificationCode(UserVerificationCodeType.EmailVerification);

        // Act
        user.AddVerificationCode(UserVerificationCodeType.EmailVerification);

        // Assert
        user.VerificationCodes.Should().HaveCount(2);
        user.VerificationCodes.Should().ContainSingle(vc => vc.Code.IsUsed);
    }

    [Fact]
    public void GetVerificationCode_Should_Return_Newest_Unused_VerificationCode()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("kjFis97823??sd"), false);
        user.AddVerificationCode(UserVerificationCodeType.EmailVerification);
        var assertCode = user.GetActiveVerificationCode(UserVerificationCodeType.EmailVerification);
        user.AddVerificationCode(UserVerificationCodeType.EmailVerification);

        // Act
        var code = user.GetActiveVerificationCode(UserVerificationCodeType.EmailVerification);

        // Assert
        code.Should().NotBeNull();
        code.Code.Should().NotBe(assertCode.Code);
    }

    [Fact]
    public void GetVerificationCode_Should_Return_Null_When_No_Codes_Present()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("kjFis97823??sd"), false);

        // Act
        var code = user.GetActiveVerificationCode(UserVerificationCodeType.EmailVerification);

        // Assert
        code.Should().BeNull();
    }

    [Fact]
    public void Verify_Should_Throw_InvalidOperationException_When_No_VerificationCodes()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("kjFis97823??sd"), false);

        // Act
        Action act = () => user.VerifyAccount("1234");

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("This chat user does not have a verification code");
    }

    [Fact]
    public void Verify_Should_Throw_InvalidOperationException_When_User_Is_Already_Verified()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("kjFis97823??sd"), true);
        user.AddVerificationCode(UserVerificationCodeType.EmailVerification);

        // Act
        Action act = () => user.VerifyAccount("1234");

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("This chat user is already verified");
    }

    [Fact]
    public void Verify_Should_Return_Fail_Result_When_Invalid_VerificationCode()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("kjFis97823??sd"), false);
        user.AddVerificationCode(UserVerificationCodeType.EmailVerification);

        // Act
        var result = user.VerifyAccount("1234");

        // Assert
        result.Success.Should().BeFalse();
        result.Error!.Description.Should().Be("Invalid verification code");
    }

    [Fact]
    public void Verify_Should_Return_Success_Result_When_Valid_VerificationCode()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("asdoiasd212?K"), false);
        user.AddVerificationCode(UserVerificationCodeType.EmailVerification);
        var code = user.GetActiveVerificationCode(UserVerificationCodeType.EmailVerification);

        // Act
        var result = user.VerifyAccount(code!.Code.Value);

        // Assert
        result.Success.Should().BeTrue();
        user.IsVerified.Should().BeTrue();
        user.VerificationCodes.Should().ContainSingle(vc => vc.Code.IsUsed);
        user.DomainEvents.Should().HaveCount(1);
    }
}
