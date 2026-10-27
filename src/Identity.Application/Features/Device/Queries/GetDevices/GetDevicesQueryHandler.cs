using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Device.Queries.GetDevices;

public class GetDevicesQueryHandler(
    IUserRepository userRepository,
    ILogger<GetDevicesQueryHandler> logger)
    : IQueryHandler<GetDevicesQuery, GetDevicesDto>
{

    public async ValueTask<Result<GetDevicesDto>> Handle(GetDevicesQuery q, CancellationToken ct)
    {
        var user = await userRepository.GetSinleAsync(new UserWithDevicesSpec(q.UserId), ct);
        if (user is null)
        {
            logger.LogCritical("User not found in GetDevicesQueryHandler for user with ID: {UserId}", q.UserId);
            return Result.Fail<GetDevicesDto>(Errors.General.NotFound(q.UserId));
        }

        var devices = user.GetDevices()
            .Select(d => new DeviceDto(d.Id, d.Name, d.Type.ToString(), d.Platform.ToString(), d.LastSeen))
            .ToList();

        return Result.Ok(new GetDevicesDto(devices));
    }
}
