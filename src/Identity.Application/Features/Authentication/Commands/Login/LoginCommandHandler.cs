using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.RateLimiting;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Common;
using Identity.Domain.Services.User;

namespace Identity.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler(
    IUserRepository userRepository,
    IAuthenticationService authenticationService,
    IUnitOfWork unitOfWork,
    IRateLimiter rateLimiter)
    : ICommandHandler<LoginCommand, LoginDto>
{
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromHours(1);
    private const int RateLimitCount = 10;

    private static readonly TimeSpan PasswordAttemptWindow = TimeSpan.FromMinutes(15);
    private const int PasswordAttemptCount = 5;

    public async ValueTask<Result<LoginDto>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        // Every attempt is counted per email before the password is checked, so concurrent guesses can't slip past the limit.
        // It is keyed on the email rather than the user, so unknown emails are limited the same way and the 429 doesn't reveal which accounts exist.
        var passwordAttemptKey = $"login:password:{cmd.Email.Trim().ToLowerInvariant()}";
        var attemptAllowed = await rateLimiter.IsAllowedAsync(passwordAttemptKey, PasswordAttemptCount, PasswordAttemptWindow, ct);
        if (attemptAllowed is false)
        {
            return Result.Fail<LoginDto>(Errors.General.TooManyRequests());
        }

        var user = await userRepository.GetSinleAsync(new UserByEmailSpec(cmd.Email), ct);
        if (user is null) return Result.Fail<LoginDto>(Errors.General.UnspecifiedError("Invalid Credentials"));

        var pwResult = user.Password.Verify(cmd.Password);
        if (!pwResult) return Result.Fail<LoginDto>(Errors.General.UnspecifiedError("Invalid Credentials"));

        // The password was correct, so only failed attempts are left counting against the limit.
        await rateLimiter.ResetAsync(passwordAttemptKey, ct);

        if (!user.IsVerified) return Result.Ok(new LoginDto { IsVerified = false, UserId = user.Id });

        var allowed = await rateLimiter.IsAllowedAsync($"login:issue:{user.Id}", RateLimitCount, RateLimitWindow, ct);
        if (allowed is false)
        {
            return Result.Fail<LoginDto>(Errors.General.TooManyRequests());
        }

        authenticationService.GenerateLoginVerificationCode(user);

        await unitOfWork.SaveChangesAsync(ct);

        return new LoginDto { IsVerified = true, UserId = user.Id };
    }
}
