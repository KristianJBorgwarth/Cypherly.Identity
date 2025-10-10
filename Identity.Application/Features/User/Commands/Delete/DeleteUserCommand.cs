using Identity.Application.Abstractions;

namespace Identity.Application.Features.User.Commands.Delete;

public sealed record DeleteUserCommand : ICommand
{
    public required Guid Id { get; init; }
}
