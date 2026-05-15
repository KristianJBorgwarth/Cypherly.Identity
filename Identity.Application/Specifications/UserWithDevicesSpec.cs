using Identity.Application.Abstractions;
using Identity.Domain.Aggregates;

internal sealed class UserWithDevicesSpec : Specification<User>
{
    public UserWithDevicesSpec(Guid id) : base(u => u.Id == id)
    {
        AddIncludes($"{nameof(User.Devices)}");
    }
}
