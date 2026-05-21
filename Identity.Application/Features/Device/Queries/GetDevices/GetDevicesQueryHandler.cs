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
    public async Task<Result<GetDevicesDto>> Handle(GetDevicesQuery q, CancellationToken ct)
    {
        var user = await userRepository.GetSinleAsync(new UserWithDevicesSpec(q.UserId), ct);
        if (user is null)
        {
            logger.LogCritical("User not found in GetDevicesQueryHandler for user with ID: {UserId}", q.UserId);
            return Result.Fail<GetDevicesDto>(Error.NotFound<Identity.Domain.Aggregates.User>(q.UserId.ToString()));
        }

        var devices = user.GetDevices();

        var dto = GetDevicesDto.Map(devices);

        return Result.Ok(dto);
    }
}
