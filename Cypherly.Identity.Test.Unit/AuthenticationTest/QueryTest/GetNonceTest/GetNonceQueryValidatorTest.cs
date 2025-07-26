using Cypherly.Identity.Application.Features.Authentication.Queries.GetNonce;
using FluentAssertions;

namespace Cypherly.Authentication.Test.Unit.AuthenticationTest.QueryTest.GetNonceTest;

public class GetNonceQueryValidatorTest
{
    private readonly GetNonceQueryValidator _sut = new();

    [Fact]
    public void GivenValidNonceQuery_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var query = new GetNonceQuery
        {
            UserId = Guid.NewGuid(),
            DeviceId = Guid.NewGuid()
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GivenInvalidNonceQuery_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var query = new GetNonceQuery
        {
            UserId = Guid.Empty,
            DeviceId = Guid.Empty
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GivenNullNonceQuery_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var query = new GetNonceQuery
        {
            UserId = Guid.NewGuid(),
            DeviceId = Guid.Empty
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}