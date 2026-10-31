using Cendrite.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Caching.LoginNonce;
using Identity.Application.Contracts.Cache;
using Identity.Application.Contracts.RateLimiting;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Common;
using Identity.Domain.Enums;
using Identity.Domain.Services.User;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.User.Commands.Update.ResendVerificationCode;

public sealed class ResendVerificationCodeCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IVerificationCodeService verificationCodeService,
    IRateLimiter rateLimiter,
    ILoginNonceCache loginNonceCache,
    ILogger<ResendVerificationCodeCommandHandler> logger)
    : ICommandHandler<ResendVerificationCodeCommand>
{
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromHours(1);
    private const int RateLimitCount = 5;

    public async ValueTask<Result> Handle(ResendVerificationCodeCommand cmd, CancellationToken ct)
    {
        return cmd.CodeType switch
        {
            UserVerificationCodeType.EmailVerification => await ResendEmailVerificationCode(cmd, ct),
            UserVerificationCodeType.Login => await ResendLoginCode(cmd, ct),
            _ => Result.Fail(Errors.General.UnexpectedValue(nameof(cmd.CodeType))),
        };
    }

    private async Task<Result> ResendEmailVerificationCode(ResendVerificationCodeCommand cmd, CancellationToken ct)
    {
        if (await IsRateLimited(cmd, ct)) return Result.Fail(Errors.General.TooManyRequests());

        var user = await userRepository.GetSinleAsync(new UserWithVerificationCodesSpec(cmd.UserId), ct);
        if (user is null)
        {
            logger.LogWarning("User {UserId} not found", cmd.UserId);
            return Result.Fail(Errors.General.NotFound(cmd.UserId));
        }

        if (user.IsVerified)
        {
            logger.LogWarning("User {UserId} is already verified", cmd.UserId);
            return Result.Fail(Errors.General.UnspecifiedError("User is already verified"));
        }

        verificationCodeService.GenerateVerificationCode(user, cmd.CodeType);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Ok();
    }

    private async Task<Result> ResendLoginCode(ResendVerificationCodeCommand cmd, CancellationToken ct)
    {
        var loginNonce = cmd.NonceId is null ? null : await loginNonceCache.GetNonceAsync(cmd.NonceId.Value, ct);
        if (loginNonce is null || !loginNonce.IsValidFor(cmd.UserId, cmd.Nonce ?? string.Empty, LoginNonceStage.PasswordVerified))
        {
            logger.LogWarning("Login nonce {NonceId} not found or invalid for user with ID: {UserId}", cmd.NonceId, cmd.UserId);
            return Result.Fail(Errors.General.Unauthorized());
        }

        if (await IsRateLimited(cmd, ct)) return Result.Fail(Errors.General.TooManyRequests());

        var user = await userRepository.GetSinleAsync(new UserWithVerificationCodesSpec(cmd.UserId), ct);
        if (user is null)
        {
            logger.LogWarning("User {UserId} not found", cmd.UserId);
            return Result.Fail(Errors.General.NotFound(cmd.UserId));
        }

        verificationCodeService.GenerateVerificationCode(user, cmd.CodeType);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Ok();
    }

    private async Task<bool> IsRateLimited(ResendVerificationCodeCommand cmd, CancellationToken ct)
    {
        return await rateLimiter.IsAllowedAsync($"resend:{cmd.CodeType}:{cmd.UserId}", RateLimitCount, RateLimitWindow, ct) is false;
    }
}
