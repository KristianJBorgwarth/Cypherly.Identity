using FluentValidation;

namespace Identity.Application.Features.Device.Queries.GetConnectionIdByUser;

public class GetConnectionIdsByUserQueryValidator : AbstractValidator<GetConnectionIdsByUserQuery>
{
    public GetConnectionIdsByUserQueryValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();
    }
}