using FluentAssertions;
using Identity.Domain.ValueObjects;

namespace Identity.Test.Unit.ValueObjectTest;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name+tag+sorting@example.com")]
    [InlineData("x@example.com")]
    [InlineData("example-indeed@strange-example.com")]
    [InlineData("admin@mailserver1")]
    public void GivenValidEmail_Should_CreateEmailObject(string validEmail)
    {
        // Act
        var result = Email.Create(validEmail);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Address.Should().Be(validEmail);
    }

    [Theory]
    [InlineData("")]
    [InlineData("plainaddress")]
    [InlineData("@missingusername.com")]
    [InlineData("username@.com")]
    [InlineData("username@.com.")]
    public void GivenInvalidEmail_Should_ReturnFailureResult(string invalidEmail)
    {
        // Act
        var result = Email.Create(invalidEmail);

        // Assert
        result.Success.Should().BeFalse();
        result.Error!.Description.Should().Be("Invalid email address.");
    }
}