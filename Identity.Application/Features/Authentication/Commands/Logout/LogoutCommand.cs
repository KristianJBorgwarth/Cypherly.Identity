using Identity.Application.Abstractions;

namespace Identity.Application.Features.Authentication.Commands.Logout;

public class LogoutCommand : ICommand
{
    public required Guid Id { get; init; }
    public required Guid DeviceId { get; init; }
}
