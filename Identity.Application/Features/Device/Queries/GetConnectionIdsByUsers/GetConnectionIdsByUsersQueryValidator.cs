using Cypherly.Domain.Common;
using Identity.Domain.Common;
using FluentValidation;

namespace Identity.Application.Features.Device.Queries.GetConnectionIdsByUsers;

public class GetConnectionIdsByUsersQueryValidator : AbstractValidator<GetConnectionIdsByUsersQuery>
{
    public GetConnectionIdsByUsersQueryValidator()
    {
        RuleFor(x => x.UserIds).NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(GetConnectionIdsByUsersQuery.UserIds)).Message);

        RuleForEach(x => x.UserIds).NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(Guid)).Message);
    }
}