using FluentValidation;

namespace Identity.Application.Features.User.Commands.Delete;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(DeleteUserCommand.Id)} ");
    }
}
