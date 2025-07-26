using Cypherly.Domain.Common;
using Cypherly.Identity.Application.Abstractions;
using Cypherly.Identity.Application.Contracts;
using Cypherly.Identity.Application.Contracts.Repository;
using Cypherly.Identity.Domain.Common;
using Cypherly.Identity.Domain.Enums;
using Cypherly.Identity.Domain.Services.User;
using Microsoft.Extensions.Logging;

namespace Cypherly.Identity.Application.Features.User.Commands.Update.ResendVerificationCode;

public sealed class ResendVerificationCodeCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IVerificationCodeService verificationCodeService,
    ILogger<ResendVerificationCodeCommandHandler> logger)
    : ICommandHandler<ResendVerificationCodeCommand>
{
    public async Task<Result> Handle(ResendVerificationCodeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(request.UserId);
            if (user is null)
            {
                logger.LogWarning("User {UserId} not found", request.UserId);
                return Result.Fail(Errors.General.NotFound(request.UserId));
            }

            if (user.IsVerified && request.CodeType == UserVerificationCodeType.EmailVerification)
            {
                logger.LogWarning("User {UserId} is already verified", request.UserId);
                return Result.Fail(Errors.General.UnspecifiedError("User is already verified"));
            }

            verificationCodeService.GenerateVerificationCode(user, request.CodeType);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception e)
        {
            logger.LogError(e, "An exception occurred while resending verification code for user {UserId}", request.UserId);
            return Result.Fail(Errors.General.UnspecifiedError("An exception occurred while resending verification code for user"));
        }
    }
}