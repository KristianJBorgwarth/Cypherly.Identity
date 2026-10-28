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
    public async ValueTask<Result<GetNonceDto>> Handle(GetNonceQuery q, CancellationToken ct)
    {
        var user = await userRepository.GetSinleAsync(new UserWithDevicesSpec(q.UserId, excludeDeleted: true), ct);
        var device = user?.GetDevices(excludeDeleted: false).FirstOrDefault(d => d.Id == q.DeviceId);

        if (user is null || device is null)
        {
            logger.LogWarning("User {UserId} or device {DeviceId} not found", q.UserId, q.DeviceId);
            return Result.Fail<GetNonceDto>(Errors.General.Unauthorized());
        }

        var nonce = Nonce.Create(user.Id, device.Id);

        await nonceCache.AddNonceAsync(nonce, ct);

        var dto = GetNonceDto.Map(nonce);

        return Result.Ok(dto);
    }
}
