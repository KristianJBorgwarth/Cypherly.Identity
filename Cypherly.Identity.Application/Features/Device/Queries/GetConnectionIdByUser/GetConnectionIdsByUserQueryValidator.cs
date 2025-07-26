using FluentValidation;

namespace Cypherly.Identity.Application.Features.Device.Queries.GetConnectionIdByUser;

public class GetConnectionIdsByUserQueryValidator : AbstractValidator<GetConnectionIdsByUserQuery>
{
    public GetConnectionIdsByUserQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}