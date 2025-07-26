using Identity.Application.Abstractions;

namespace Identity.Application.Features.User.Commands.Delete;

public sealed record DeleteUserCommand : ICommandId
{
    public required Guid Id { get; init; }
}