using FluentValidation;

namespace Identity.Application.Features.User.Commands.Create;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(CreateUserCommand.Email)} ");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(CreateUserCommand.Password)} ");
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(CreateUserCommand.Username)} ")
            .Must(x => x.Length >= 3).WithMessage($"Value '{nameof(CreateUserCommand.Username)}' should be at least 3.")
            .Must(x => x.Length <= 20).WithMessage($"Value '{nameof(CreateUserCommand.Username)}' should not exceed 20.");
    }
}
