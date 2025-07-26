using Cypherly.Domain.Common;
using Cypherly.Identity.Application.Abstractions;
using Cypherly.Identity.Application.Caching;
using Cypherly.Identity.Application.Contracts;
using Cypherly.Identity.Application.Contracts.Cache;
using Cypherly.Identity.Application.Contracts.Repository;
using Cypherly.Identity.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Cypherly.Identity.Application.Features.Authentication.Queries.GetNonce;

public class GetNonceQueryHandler(
    IUserRepository userRepository,
    INonceCacheService nonceCache,
    ILogger<GetNonceQueryHandler> logger)
    : IQueryHandler<GetNonceQuery, GetNonceDto>
{
    public async Task<Result<GetNonceDto>> Handle(GetNonceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(request.UserId);

            if (user is null)
            {
                logger.LogWarning("User with ID: {ID} not found.", request.UserId);
                return Result.Fail<GetNonceDto>(Errors.General.NotFound(request.UserId));
            }

            var device = user.GetDevice(request.DeviceId);

            var nonce = Nonce.Create(user.Id, device.Id);

            await nonceCache.AddNonceAsync(nonce, cancellationToken);

            var dto = GetNonceDto.Map(nonce);

            return Result.Ok(dto);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Exception occurred attempting to fetch nonce for user with ID: {UserId} and Device ID: {DeviceId}.", request.UserId, request.DeviceId);
            return Result.Fail<GetNonceDto>(Errors.General.UnspecifiedError("An exception occured attempting to fetch nonce."));
        }
    }
}