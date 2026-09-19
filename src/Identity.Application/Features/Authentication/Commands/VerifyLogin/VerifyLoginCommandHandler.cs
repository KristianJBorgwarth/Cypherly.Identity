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

    public async Task<Result<VerifyLoginDto>> Handle(VerifyLoginCommand cmd, CancellationToken ct)
    {
        var allowed = await rateLimiter.IsAllowedAsync($"login:verify:{cmd.UserId}", RateLimitCount, RateLimitWindow, ct);
        if (allowed is false)
        {
            return Result.Fail<VerifyLoginDto>(Errors.General.TooManyRequests());
        }

        var user = await userRepository.GetSinleAsync(new UserWithVerificationCodesSpec(cmd.UserId), ct);
        if (user is null)
        {
            logger.LogWarning("User with ID {UserId} not found", cmd.UserId);
            return Result.Fail<VerifyLoginDto>(Errors.General.NotFound(cmd.UserId));
        }

        var verificationResult = user.VerifyLogin(cmd.LoginVerificationCode);
        if (verificationResult.Success is false)
        {
            // Persist the attempt count / burn even though verification failed.
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Fail<VerifyLoginDto>(verificationResult.Error);
        }

        var loginNonce = LoginNonce.Create(cmd.UserId);

        await loginNonceCache.AddNonceAsync(loginNonce, ct);

        var dto = VerifyLoginDto.Map(loginNonce);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Ok(dto);
    }
}
