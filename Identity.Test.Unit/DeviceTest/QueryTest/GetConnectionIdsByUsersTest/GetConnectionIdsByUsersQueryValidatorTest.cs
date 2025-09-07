using Identity.Application.Features.Device.Queries.GetConnectionIdsByUsers;
using FluentAssertions;
using Identity.Domain.Common;

namespace Cypherly.Authentication.Test.Unit.DeviceTest.QueryTest.GetConnectionIdsByUsersTest;

public class GetConnectionIdsByUsersQueryValidatorTest
{
    private readonly GetConnectionIdsByUsersQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenUserIdsAreProvided_ShouldBeValid()
    {
        // Arrange
        var query = new GetConnectionIdsByUsersQuery
        {
            TenantIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenUserIdsAreEmpty_ShouldBeInvalid()
    {
        // Arrange
        var query = new GetConnectionIdsByUsersQuery
        {
            TenantIds = new List<Guid>()
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(Errors.General.ValueIsEmpty(nameof(GetConnectionIdsByUsersQuery.TenantIds)).Message);
    }

    [Fact]
    public void Validate_WhenUserIdsContainEmptyGuid_ShouldBeInvalid()
    {
        // Arrange
        var query = new GetConnectionIdsByUsersQuery
        {
            TenantIds = new List<Guid> { Guid.NewGuid(), Guid.Empty }
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(Errors.General.ValueIsEmpty(nameof(Guid)).Message);
    }
}