using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Device.Queries.GetConnectionIdByUser;

public class GetConnectionIdsByUserQueryHandler(
    IUserRepository userRepository,
    ILogger<GetConnectionIdsByUserQueryHandler> logger)
    : IQueryHandler<GetConnectionIdsByUserQuery, GetConnectionIdsByUserDto>
{
    public async Task<Result<GetConnectionIdsByUserDto>> Handle(GetConnectionIdsByUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(request.TenantId);
            if (user is null) return Result.Fail<GetConnectionIdsByUserDto>(Errors.General.NotFound(request.TenantId));
            
            var connectionIds = user.GetDevices().Select(x => x.ConnectionId).ToList(); 
            
            return new GetConnectionIdsByUserDto { ConnectionIds = connectionIds };
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception, "Error retrieving connection ids for user {UserId}", request.TenantId);
            return Result.Fail<GetConnectionIdsByUserDto>(Errors.General.UnspecifiedError("An exception occurred while retrieving connection ids for user"));
        }
    }
}