using Identity.Application.Features.Device.Queries.GetDevices;
using FluentAssertions;

namespace Cypherly.Authentication.Test.Unit.DeviceTest.QueryTest.GetDevicesTest;

public class GetDevicesQueryValidatorTest
{
    private readonly GetDevicesQueryValidator _sut = new();

    [Fact]
    public void Given_Valid_Query_ShouldNotReturnError()
    {
        // Arrange
        var query = new GetDevicesQuery()
        {
            UserId = Guid.NewGuid()
        };

        // Act
        var result = _sut.Validate(query);

        // Arrange
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_Invalid_Query_ShouldReturnError()
    {
        // Arrange
        var query = new GetDevicesQuery()
        {
            UserId = Guid.Empty
        };

        // Act
        var result = _sut.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
    }
}