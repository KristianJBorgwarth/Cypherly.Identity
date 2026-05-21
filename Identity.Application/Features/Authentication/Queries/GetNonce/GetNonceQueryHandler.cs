using Identity.Application.Abstractions;
using Identity.Application.Caching;
using Identity.Application.Contracts.Cache;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Aggregates;
using Identity.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Authentication.Queries.GetNonce;

public class GetNonceQueryHandler(
    IUserRepository userRepository,
    INonceCacheService nonceCache,
    ILogger<GetNonceQueryHandler> logger)
    : IQueryHandler<GetNonceQuery, GetNonceDto>
{
    public async Task<Result<GetNonceDto>> Handle(GetNonceQuery q, CancellationToken ct)
    {
        var user = await userRepository.GetSinleAsync(new UserWithDevicesSpec(q.UserId), ct);

        if (user is null)
        {
            logger.LogWarning("User with ID: {ID} not found.", q.UserId);
            return Result.Fail<GetNonceDto>(Error.NotFound<User>(q.UserId.ToString()));
        }

        var device = user.GetDevice(q.DeviceId);

        var nonce = Nonce.Create(user.Id, device.Id);

        await nonceCache.AddNonceAsync(nonce, ct);

        var dto = GetNonceDto.Map(nonce);

        return Result.Ok(dto);
    }
}
