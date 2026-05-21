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
    public async Task<Result<VerifyNonceDto>> Handle(VerifyNonceCommand cmd, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetSinleAsync(new UserWithDevicesSpec(cmd.UserId), cancellationToken);

        if (user is null)
        {
            logger.LogWarning("User with ID: {ID} not found.", cmd.UserId);
            return Result.Fail<VerifyNonceDto>(Errors.General.NotFound(cmd.UserId));
        }

        var nonce = await nonceCacheService.GetNonceAsync(cmd.NonceId, cancellationToken);

        if (nonce is null)
        {
            logger.LogWarning("Nonce with ID: {ID} not found.", cmd.NonceId);
            return Result.Fail<VerifyNonceDto>(Errors.General.NotFound(cmd.NonceId));
        }

        var device = user.GetDevice(cmd.DeviceId);

        var isNonceValid = verifyNonceService.VerifyNonce(nonce.NonceValue, cmd.Nonce, device.PublicKey);

        if (!isNonceValid)
            return Result.Fail<VerifyNonceDto>(Errors.General.Unauthorized());

        var token = await jwtService.GenerateTokenAsync(user.Id, cmd.DeviceId);
        device.AddRefreshToken();
        var refreshToken = device.GetActiveRefreshToken();

        var dto = VerifyNonceDto.Map(token, refreshToken!);

        user.AddDomainEvent(new UserLoggedInEvent(user.Id, device.Id, device.ConnectionId));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(dto);
    }
}
