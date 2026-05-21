using FluentValidation;

namespace Identity.Application.Features.Authentication.Commands.Logout;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage($"The value cannot be empty: {nameof(LogoutCommand.Id)} ");

        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .WithMessage($"The value cannot be empty: {nameof(LogoutCommand.DeviceId)} ");
    }
}
