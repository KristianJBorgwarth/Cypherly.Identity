using FluentValidation;

namespace Identity.Application.Features.Device.Queries.GetDevices;

public class GetDevicesQueryValidator : AbstractValidator<GetDevicesQuery>
{
    public GetDevicesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(GetDevicesQuery.UserId)} ");
    }
}
