using Identity.Application.Abstractions;
using Identity.Domain.Aggregates;

internal sealed class UserWithVerificationCodesSpec : Specification<User>
{
    public UserWithVerificationCodesSpec(Guid id, bool excludeDeleted = false) : base(u => u.Id == id && (!excludeDeleted || u.Deleted == null))
    {
        AddIncludes($"{nameof(User.VerificationCodes)}");
    }
}
