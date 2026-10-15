using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Cache;
using Identity.Application.Contracts.Repository;
using Identity.Application.Interfaces;
using Identity.Domain.Common;
using Identity.Domain.Events.User;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Authentication.Commands.VerifyNonce;

public class VerifyNonceCommandHandler(
    IUserRepository userRepository,
    INonceCacheService nonceCacheService,
    IJwtService jwtService,
    IVerifyNonceService verifyNonceService,
    IUnitOfWork unitOfWork,
    ILogger<VerifyNonceCommandHandler> logger)
    : ICommandHandler<VerifyNonceCommand, VerifyNonceDto>
{
    public async ValueTask<Result<VerifyNonceDto>> Handle(VerifyNonceCommand cmd, CancellationToken cancellationToken)
    {
        var nonce = await nonceCacheService.GetNonceAsync(cmd.NonceId, cancellationToken);

        if (nonce is null || !nonce.IsValidFor(cmd.UserId, cmd.DeviceId))
        {
            logger.LogWarning("Nonce {NonceId} not found or invalid for user {UserId} and device {DeviceId}", cmd.NonceId, cmd.UserId, cmd.DeviceId);
            return Result.Fail<VerifyNonceDto>(Errors.General.Unauthorized());
        }

        var user = await userRepository.GetSinleAsync(new UserWithDevicesSpec(cmd.UserId), cancellationToken);
        var device = user?.GetDevices().FirstOrDefault(d => d.Id == cmd.DeviceId);

        if (user is null || device is null)
        {
            logger.LogWarning("User {UserId} or device {DeviceId} not found for nonce {NonceId}", cmd.UserId, cmd.DeviceId, cmd.NonceId);
            return Result.Fail<VerifyNonceDto>(Errors.General.Unauthorized());
        }

        var isNonceValid = verifyNonceService.VerifyNonce(nonce.NonceValue, cmd.Nonce, device.PublicKey);

        if (!isNonceValid)
            return Result.Fail<VerifyNonceDto>(Errors.General.Unauthorized());

        if (await nonceCacheService.TryClaimNonceAsync(cmd.NonceId, cancellationToken) is false)
        {
            logger.LogWarning("Nonce {NonceId} for user {UserId} was already used", cmd.NonceId, cmd.UserId);
            return Result.Fail<VerifyNonceDto>(Errors.General.Unauthorized());
        }

        var token = await jwtService.GenerateTokenAsync(user.Id, cmd.DeviceId);
        device.AddRefreshToken();
        var refreshToken = device.GetActiveRefreshToken();

        var dto = VerifyNonceDto.Map(token, refreshToken!);

        user.AddDomainEvent(new UserLoggedInEvent(user.Id, device.Id, device.ConnectionId));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await nonceCacheService.DeleteNonceAsync(cmd.NonceId, cancellationToken);

        return Result.Ok(dto);
    }
}
