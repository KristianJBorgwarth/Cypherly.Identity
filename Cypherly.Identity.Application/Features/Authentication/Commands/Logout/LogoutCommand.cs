using Cypherly.Identity.Application.Abstractions;

namespace Cypherly.Identity.Application.Features.Authentication.Commands.Logout;

public class LogoutCommand : ICommandId
{
    public required Guid Id { get; init; }
    public required Guid DeviceId { get; init; }
}