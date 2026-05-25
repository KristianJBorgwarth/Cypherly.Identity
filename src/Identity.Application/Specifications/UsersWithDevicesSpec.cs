using Identity.Application.Abstractions;
using Identity.Domain.Aggregates;

namespace Identity.Application.Specifications;

public sealed class UsersWithDevicesSpec : Specification<User>
{
    public UsersWithDevicesSpec(IEnumerable<Guid> tenantIds) : base(u => tenantIds.Contains(u.Id))
    {
        AddIncludes($"{nameof(User.Devices)}");
    }
}
