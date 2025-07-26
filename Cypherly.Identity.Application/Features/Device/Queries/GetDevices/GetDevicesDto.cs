namespace Cypherly.Identity.Application.Features.Device.Queries.GetDevices;

public sealed class GetDevicesDto
{
    public List<DeviceDto> Devices { get; init; }

    private GetDevicesDto() { } // Hide constructor to enforce use of map method

    public static GetDevicesDto Map(IEnumerable<Identity.Domain.Entities.Device> devices)
    {
        return new GetDevicesDto
        {
            Devices = devices.Select(d => new DeviceDto
            {
                Id = d.Id,
                Name = d.Name,
                Type = d.Type.ToString(),
                Platform = d.Platform.ToString(),
            }).ToList(),
        };
    }
}

public sealed record DeviceDto()
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required string Platform { get; init; }
}