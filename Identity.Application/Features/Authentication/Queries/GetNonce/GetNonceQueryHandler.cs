using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Caching;
using Identity.Application.Contracts.Cache;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Authentication.Queries.GetNonce;

public class GetNonceQueryHandler(
    IUserRepository userRepository,
    INonceCacheService nonceCache,
    ILogger<GetNonceQueryHandler> logger)
    : IQueryHandler<GetNonceQuery, GetNonceDto>
{
    public async Task<Result<GetNonceDto>> Handle(GetNonceQuery request, CancellationToken cancellationToken)
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
}
