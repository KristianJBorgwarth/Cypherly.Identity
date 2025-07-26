using Cypherly.Domain.Common;
using Cypherly.Identity.Domain.Common;
using FluentValidation;

namespace Cypherly.Identity.Application.Features.Device.Queries.GetDevices;

public class GetDevicesQueryValidator : AbstractValidator<GetDevicesQuery>
{
    public GetDevicesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(GetDevicesQuery.UserId)).Message);
    }
}