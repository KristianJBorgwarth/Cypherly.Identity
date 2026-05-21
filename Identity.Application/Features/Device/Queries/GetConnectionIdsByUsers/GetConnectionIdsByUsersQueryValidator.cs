using FluentValidation;

namespace Identity.Application.Features.Device.Queries.GetConnectionIdsByUsers;

public class GetConnectionIdsByUsersQueryValidator : AbstractValidator<GetConnectionIdsByUsersQuery>
{
    public GetConnectionIdsByUsersQueryValidator()
    {
        RuleFor(x => x.TenantIds).NotEmpty()
            .WithMessage($"The value cannot be empty: {nameof(GetConnectionIdsByUsersQuery.TenantIds)} ");

        RuleForEach(x => x.TenantIds).NotEmpty()
            .WithMessage($"The value cannot be empty: {nameof(Guid)} ");
    }
}
