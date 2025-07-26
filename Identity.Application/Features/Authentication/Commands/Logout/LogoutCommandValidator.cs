using Cypherly.Domain.Common;
using Identity.Domain.Common;
using FluentValidation;

namespace Identity.Application.Features.Authentication.Commands.Logout;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(LogoutCommand.Id)).Message);

        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(LogoutCommand.DeviceId)).Message);
    }
}