using Identity.Application.Abstractions;
using Identity.Domain.Aggregates;
using Identity.Domain.Entities;

internal sealed class UserWithDeviceAndRefreshTokensSpec : Specification<User>
{
    public UserWithDeviceAndRefreshTokensSpec(Guid id) : base(u => u.Id == id)
    {
        AddIncludes($"{nameof(User.Devices)}.{nameof(Device.RefreshTokens)}");
    }
}
