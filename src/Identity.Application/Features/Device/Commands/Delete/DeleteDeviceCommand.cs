using Identity.Application.Abstractions;

namespace Identity.Application.Features.Device.Commands.Delete;

public sealed record DeleteDeviceCommand : ICommand
{
    public required Guid UserId { get; init; }
    public required Guid DeviceId { get; init; }
}
