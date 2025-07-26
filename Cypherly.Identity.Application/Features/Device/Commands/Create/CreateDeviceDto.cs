namespace Cypherly.Identity.Application.Features.Device.Commands.Create;

public sealed class CreateDeviceDto
{
    public Guid DeviceId { get; init; }
    public Guid DeviceConnectionId { get; init; }

    private CreateDeviceDto() { } // Hide the constructor to force the use of the Map method

    public static CreateDeviceDto Map(Identity.Domain.Entities.Device device)
    {
        return new CreateDeviceDto()
        {
            DeviceId = device.Id,
            DeviceConnectionId = device.ConnectionId,
        };
    }
}