using Cypherly.Domain.Common;
using Identity.Application.Contracts;
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

    public async Task<Result<GetDevicesDto>> Handle(GetDevicesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(request.UserId);
            if (user is null)
            {
                logger.LogCritical("User not found in GetDevicesQueryHandler for user with ID: {UserId}", request.UserId);
                return Result.Fail<GetDevicesDto>(Errors.General.NotFound(request.UserId));
            }

            var devices = user.GetDevices();

            var dto = GetDevicesDto.Map(devices);

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetDevicesQueryHandler");
            return Result.Fail<GetDevicesDto>(Errors.General.UnspecifiedError("An exception occured while fetching devices"));
        }
    }
}