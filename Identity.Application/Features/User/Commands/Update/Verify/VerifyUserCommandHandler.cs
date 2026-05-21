using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Aggregates;
using Identity.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.User.Commands.Update.Verify;

public class VerifyUserCommandHandler(
    ILogger<VerifyUserCommandHandler> logger,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<VerifyUserCommand>
{
    public async Task<Result> Handle(VerifyUserCommand cmd, CancellationToken ct)
    {
        var user = await userRepository.GetSinleAsync(new UserWithVerificationCodesSpec(cmd.UserId), ct);
        if (user is null)
        {
            logger.LogWarning("User with ID {UserId} not found during verification process", cmd.UserId);
            return Result.Fail(Error.NotFound<User>(cmd.UserId.ToString()));
        }

        var result = user.VerifyAccount(cmd.VerificationCode);
        if (result.Success is false) return Result.Fail(result.Error);

        await userRepository.UpdateAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
