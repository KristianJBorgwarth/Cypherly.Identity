using Cypherly.Identity.Application.Abstractions;

namespace Cypherly.Identity.Application.Features.User.Commands.Delete;

public sealed record DeleteUserCommand : ICommandId
{
    public required Guid Id { get; init; }
}