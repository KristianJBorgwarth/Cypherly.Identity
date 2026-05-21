using Identity.Application.Abstractions;
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
    ILogger<ResendVerificationCodeCommandHandler> logger)
    : ICommandHandler<ResendVerificationCodeCommand>
{
    public async Task<Result> Handle(ResendVerificationCodeCommand cmd, CancellationToken ct)
    {
        var user = await userRepository.GetSinleAsync(new UserWithVerificationCodesSpec(cmd.UserId), ct);
        if (user is null)
        {
            logger.LogWarning("User {UserId} not found", cmd.UserId);
            return Result.Fail(Error.NotFound<Domain.Aggregates.User>(cmd.UserId.ToString()));
        }

        if (user.IsVerified && cmd.CodeType == UserVerificationCodeType.EmailVerification)
        {
            logger.LogWarning("User {UserId} is already verified", cmd.UserId);
            return Result.Fail(Error.BadRequest("user.already.verified", "User is already verified"));
        }

        verificationCodeService.GenerateVerificationCode(user, cmd.CodeType);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
