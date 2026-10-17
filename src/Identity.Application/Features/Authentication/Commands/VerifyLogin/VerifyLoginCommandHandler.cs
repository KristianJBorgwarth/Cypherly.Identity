using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Caching.LoginNonce;
using Identity.Application.Contracts.Cache;
using Identity.Application.Contracts.RateLimiting;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Authentication.Commands.VerifyLogin;

public sealed class VerifyLoginCommandHandler(
    IUserRepository userRepository,
    ILoginNonceCache loginNonceCache,
    IUnitOfWork unitOfWork,
    IRateLimiter rateLimiter,
    ILogger<VerifyLoginCommandHandler> logger)
    : ICommandHandler<VerifyLoginCommand, VerifyLoginDto>
{
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromHours(1);
    private const int RateLimitCount = 10;

    public async ValueTask<Result<VerifyLoginDto>> Handle(VerifyLoginCommand cmd, CancellationToken ct)
    {
        var passwordNonce = await loginNonceCache.GetNonceAsync(cmd.NonceId, ct);
        if (passwordNonce is null || !passwordNonce.IsValidFor(cmd.UserId, cmd.Nonce, LoginNonceStage.PasswordVerified))
        {
            logger.LogWarning("Login nonce {NonceId} not found or invalid for user with ID: {UserId}", cmd.NonceId, cmd.UserId);
            return Result.Fail<VerifyLoginDto>(Errors.General.Unauthorized());
        }

        var allowed = await rateLimiter.IsAllowedAsync($"login:verify:{cmd.UserId}", RateLimitCount, RateLimitWindow, ct);
        if (allowed is false)
        {
            return Result.Fail<VerifyLoginDto>(Errors.General.TooManyRequests());
        }

        var user = await userRepository.GetSinleAsync(new UserWithVerificationCodesSpec(cmd.UserId, excludeDeleted: true), ct);
        if (user is null)
        {
            logger.LogWarning("User with ID {UserId} not found or deleted", cmd.UserId);
            return Result.Fail<VerifyLoginDto>(Errors.General.NotFound(cmd.UserId));
        }

        var verificationResult = user.VerifyLogin(cmd.LoginVerificationCode);
        if (verificationResult.Success is false)
        {
            // Persist the attempt count / burn even though verification failed.
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Fail<VerifyLoginDto>(verificationResult.Error);
        }

        await loginNonceCache.DeteleNonceAsync(cmd.NonceId, ct);

        var loginNonce = LoginNonce.Create(cmd.UserId, LoginNonceStage.CodeVerified);

        await loginNonceCache.AddNonceAsync(loginNonce, ct);

        var dto = VerifyLoginDto.Map(loginNonce);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Ok(dto);
    }
}
