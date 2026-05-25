using Identity.Application.Abstractions;
using Identity.Domain.Aggregates;

internal sealed class UserByDeviceIdWithDevicesSpec : Specification<User>
{
    public UserByDeviceIdWithDevicesSpec(Guid deviceId) : base(u => u.Devices.Any(d => d.Id == deviceId))
    {
        AddIncludes($"{nameof(User.Devices)}");
    }
}
