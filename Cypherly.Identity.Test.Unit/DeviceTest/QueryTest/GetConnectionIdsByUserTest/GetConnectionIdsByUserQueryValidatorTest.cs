using Cypherly.Identity.Application.Features.Device.Queries.GetConnectionIdByUser;
using FluentAssertions;

namespace Cypherly.Authentication.Test.Unit.DeviceTest.QueryTest.GetConnectionIdsByUserTest;

public class GetConnectionIdsByUserQueryValidatorTest
{
    private readonly GetConnectionIdsByUserQueryValidator _sut = new GetConnectionIdsByUserQueryValidator();

    [Fact]
    public void GivenValidQuery_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var query = new GetConnectionIdsByUserQuery
        {
            UserId = Guid.NewGuid()
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GivenEmptyUserId_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var query = new GetConnectionIdsByUserQuery
        {
            UserId = Guid.Empty
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors[0].ErrorMessage.Should().Be("'User Id' must not be empty.");
    }
}