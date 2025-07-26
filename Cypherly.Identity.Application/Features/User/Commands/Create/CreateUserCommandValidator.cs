using Cypherly.Domain.Common;
using Cypherly.Identity.Domain.Common;
using FluentValidation;

namespace Cypherly.Identity.Application.Features.User.Commands.Create;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(CreateUserCommand.Email)).Message);
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(CreateUserCommand.Password)).Message);
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(CreateUserCommand.Username)).Message)
            .Must(x => x.Length >= 3).WithMessage(Errors.General.ValueTooSmall(nameof(CreateUserCommand.Username), 3).Message)
            .Must(x => x.Length <= 20).WithMessage(Errors.General.ValueTooLarge(nameof(CreateUserCommand.Username), 20).Message);
    }
}