using Cypherly.Domain.Common;
using Cypherly.Identity.Application.Abstractions;
using Cypherly.Identity.Application.Contracts;
using Cypherly.Identity.Application.Contracts.Repository;
using Cypherly.Identity.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Cypherly.Identity.Application.Features.Device.Queries.GetConnectionIdsByUsers;

public class GetConnectionIdsByUsersQueryHandler(
    IUserRepository userRepository,
    ILogger<GetConnectionIdsByUsersQueryHandler> logger)
    : IQueryHandler<GetConnectionIdsByUsersQuery, GetConnectionIdsByUsersDto>
{

    public async Task<Result<GetConnectionIdsByUsersDto>> Handle(GetConnectionIdsByUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await userRepository.GetUsersAsync(request.UserIds.ToArray());
            var connectionIds = GetConnectionIdsByUsersDto.MapFrom(users);
            return Result.Ok(connectionIds);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "exception occured while attempting to get connection ids by users");
            return Result.Fail<GetConnectionIdsByUsersDto>(Errors.General.UnspecifiedError("An error occured while attempting to get connection ids by users"));
        }
    }
}