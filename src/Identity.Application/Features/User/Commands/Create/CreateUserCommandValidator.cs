using Cypherly.Domain.Common;
using Identity.Domain.Common;
using FluentValidation;

namespace Identity.Application.Features.User.Commands.Create;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(CreateUserCommand.Email)).Description);
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(CreateUserCommand.Password)).Description);
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(CreateUserCommand.Username)).Description)
            .Must(x => x.Length >= 3).WithMessage(Errors.General.ValueTooSmall(nameof(CreateUserCommand.Username), 3).Description)
            .Must(x => x.Length <= 20).WithMessage(Errors.General.ValueTooLarge(nameof(CreateUserCommand.Username), 20).Description);
    }
}