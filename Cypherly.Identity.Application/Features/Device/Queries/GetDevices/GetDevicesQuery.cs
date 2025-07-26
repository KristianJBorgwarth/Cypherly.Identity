using Cypherly.Identity.Application.Abstractions;

namespace Cypherly.Identity.Application.Features.Device.Queries.GetDevices;

public sealed record GetDevicesQuery : IQuery<GetDevicesDto>
{
    public required Guid UserId { get; init; }
}