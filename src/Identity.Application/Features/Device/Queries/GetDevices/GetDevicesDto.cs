namespace Identity.Application.Features.Device.Queries.GetDevices;

#pragma warning disable IDE0290 // Use primary constructor
public sealed class GetDevicesDto
{
    public IReadOnlyCollection<DeviceDto> Devices { get; private set; }

    public GetDevicesDto(IReadOnlyCollection<DeviceDto> devices)
    {
        Devices = devices;
    }
}

public sealed class DeviceDto
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Type { get; private set; }
    public string Platform { get; private set; }
    public DateTime? LastSeen { get; private set; }

    public DeviceDto(Guid id, string name, string type, string platform, DateTime? lastSeen)
    {
        Id = id;
        Name = name;
        Type = type;
        Platform = platform;
        LastSeen = lastSeen;
    }
}
