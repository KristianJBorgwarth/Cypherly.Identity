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
    ITokenIssuer tokenIssuer,
    IVerifyNonceService verifyNonceService,
    IUnitOfWork unitOfWork,
    ILogger<VerifyNonceCommandHandler> logger)
    : ICommandHandler<VerifyNonceCommand, VerifyNonceDto>
{
    public async ValueTask<Result<VerifyNonceDto>> Handle(VerifyNonceCommand cmd, CancellationToken ct)
    {
        var nonce = await nonceCacheService.GetNonceAsync(cmd.NonceId, ct);

        if (nonce is null || !nonce.IsValidFor(cmd.UserId, cmd.DeviceId))
        {
            logger.LogWarning("Nonce {NonceId} not found or invalid for user {UserId} and device {DeviceId}", cmd.NonceId, cmd.UserId, cmd.DeviceId);
            return Result.Fail<VerifyNonceDto>(Errors.General.Unauthorized());
        }

        var user = await userRepository.GetSinleAsync(new UserWithDevicesSpec(cmd.UserId, excludeDeleted: true), ct);
        var device = user?.GetDevices().FirstOrDefault(d => d.Id == cmd.DeviceId);

        if (user is null || device is null)
        {
            logger.LogWarning("User {UserId} or device {DeviceId} not found or deleted for nonce {NonceId}", cmd.UserId, cmd.DeviceId, cmd.NonceId);
            return Result.Fail<VerifyNonceDto>(Errors.General.Unauthorized());
        }

        var isNonceValid = verifyNonceService.VerifyNonce(nonce.NonceValue, cmd.Nonce, device.PublicKey);

        if (!isNonceValid)
            return Result.Fail<VerifyNonceDto>(Errors.General.Unauthorized());

        if (await nonceCacheService.TryClaimNonceAsync(cmd.NonceId, ct) is false)
        {
            logger.LogWarning("Nonce {NonceId} for user {UserId} was already used", cmd.NonceId, cmd.UserId);
            return Result.Fail<VerifyNonceDto>(Errors.General.Unauthorized());
        }

        var token = await jwtService.GenerateTokenAsync(user.Id, cmd.DeviceId, device.ConnectionId);

        if (cmd.Announce)
            user.AddDomainEvent(new UserLoggedInEvent(user.Id, device.Id, device.ConnectionId));

        await unitOfWork.SaveChangesAsync(ct);

        await nonceCacheService.DeleteNonceAsync(cmd.NonceId, ct);

        var tokenResponses = await tokenIssuer.IssueAsync(user.Id, cmd.TokenRequests, ct);

        return Result.Ok(new VerifyNonceDto(token.Token, token.ExpiresAt, tokenResponses));
    }
}
