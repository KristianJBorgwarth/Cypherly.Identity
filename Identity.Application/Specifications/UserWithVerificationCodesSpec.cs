using Identity.Application.Abstractions;
using Identity.Domain.Aggregates;

internal sealed class UserWithVerificationCodesSpec : Specification<User>
{
    public UserWithVerificationCodesSpec(Guid id) : base(u => u.Id == id)
    {
        AddIncludes($"{nameof(User.VerificationCodes)}");
    }
}
