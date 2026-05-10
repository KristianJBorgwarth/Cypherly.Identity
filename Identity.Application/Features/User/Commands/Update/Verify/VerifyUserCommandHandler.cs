using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.User.Commands.Update.Verify;

public class VerifyUserCommandHandler(
    ILogger<VerifyUserCommandHandler> logger,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<VerifyUserCommand>
{
    public async Task<Result> Handle(VerifyUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("User with ID {UserId} not found during verification process", request.UserId);
            return Result.Fail(Errors.General.NotFound(request.UserId));
        }

        var result = user.VerifyAccount(request.VerificationCode);
        if (result.Success is false) return Result.Fail(result.Error);

        await userRepository.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
