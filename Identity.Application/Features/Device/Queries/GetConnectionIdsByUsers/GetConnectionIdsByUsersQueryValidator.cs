using Cypherly.Domain.Common;
using Identity.Domain.Common;
using FluentValidation;

namespace Identity.Application.Features.Device.Queries.GetConnectionIdsByUsers;

public class GetConnectionIdsByUsersQueryValidator : AbstractValidator<GetConnectionIdsByUsersQuery>
{
    public GetConnectionIdsByUsersQueryValidator()
    {
        RuleFor(x => x.TenantIds).NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(GetConnectionIdsByUsersQuery.TenantIds)).Message);

        RuleForEach(x => x.TenantIds).NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(Guid)).Message);
    }
}