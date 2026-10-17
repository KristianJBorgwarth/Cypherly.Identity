using Identity.Application.Abstractions;
using Identity.Domain.Aggregates;

internal sealed class UserWithDevicesSpec : Specification<User>
{
    public UserWithDevicesSpec(Guid id, bool excludeDeleted = false) : base(u => u.Id == id && (!excludeDeleted || u.Deleted == null))
    {
        AddIncludes($"{nameof(User.Devices)}");
    }
}
