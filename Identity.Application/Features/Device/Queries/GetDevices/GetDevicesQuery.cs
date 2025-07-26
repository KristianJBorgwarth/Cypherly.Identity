using Identity.Application.Abstractions;

namespace Identity.Application.Features.Device.Queries.GetDevices;

public sealed record GetDevicesQuery : IQuery<GetDevicesDto>
{
    public required Guid UserId { get; init; }
}