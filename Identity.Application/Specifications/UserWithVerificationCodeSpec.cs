using Identity.Application.Abstractions;
using Identity.Domain.Aggregates;

internal sealed class UserWithVerificationCodeSpec : Specification<User>
{
    public UserWithVerificationCodeSpec(Guid id) : base(u => u.Id == id)
    {
        AddIncludes(nameof(User.VerificationCodes));
    }
}
