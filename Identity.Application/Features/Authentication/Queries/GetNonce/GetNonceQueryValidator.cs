using FluentValidation;

namespace Identity.Application.Features.Authentication.Queries.GetNonce;

public class GetNonceQueryValidator : AbstractValidator<GetNonceQuery>
{
    public GetNonceQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(GetNonceQuery.UserId)} ");

        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(GetNonceQuery.DeviceId)} ");
    }
}
