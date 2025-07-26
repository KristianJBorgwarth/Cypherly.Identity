using Cypherly.Domain.Common;
using Cypherly.Identity.Application.Abstractions;
using Cypherly.Identity.Application.Contracts;
using Cypherly.Identity.Application.Contracts.Repository;
using Cypherly.Identity.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Cypherly.Identity.Application.Features.Device.Queries.GetConnectionIdByUser;

public class GetConnectionIdsByUserQueryHandler(
    IUserRepository userRepository,
    ILogger<GetConnectionIdsByUserQueryHandler> logger)
    : IQueryHandler<GetConnectionIdsByUserQuery, GetConnectionIdsByUserDto>
{
    public async Task<Result<GetConnectionIdsByUserDto>> Handle(GetConnectionIdsByUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(request.UserId);
            if (user is null)
            {
                return Result.Fail<GetConnectionIdsByUserDto>(Errors.General.NotFound(request.UserId));
            }
            var dto = GetConnectionIdsByUserDto.MapFrom(user.GetDevices());
            return Result.Ok(dto);
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception, "Error retrieving connection ids for user {UserId}", request.UserId);
            return Result.Fail<GetConnectionIdsByUserDto>(Errors.General.UnspecifiedError("An exception occurred while retrieving connection ids for user"));
        }
    }
}