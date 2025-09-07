using Cypherly.Domain.Common;
using Identity.Application.Contracts;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Device.Queries.GetConnectionIdsByUsers;

public class GetConnectionIdsByUsersQueryHandler(
    IUserRepository userRepository,
    ILogger<GetConnectionIdsByUsersQueryHandler> logger)
    : IQueryHandler<GetConnectionIdsByUsersQuery, GetConnectionIdsByUsersDto>
{

    public async Task<Result<GetConnectionIdsByUsersDto>> Handle(GetConnectionIdsByUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await userRepository.GetUsersAsync(request.TenantIds.ToArray());
            
            var connectionIds = users.ToDictionary(
                user => user.Id,
                user => user.GetDevices().Select(device => device.ConnectionId).ToList());
            
            var dto = new GetConnectionIdsByUsersDto() {ConnectionIds = connectionIds};

            return dto;
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "exception occured while attempting to get connection ids by users");
            return Result.Fail<GetConnectionIdsByUsersDto>(Errors.General.UnspecifiedError("An error occured while attempting to get connection ids by users"));
        }
    }
}