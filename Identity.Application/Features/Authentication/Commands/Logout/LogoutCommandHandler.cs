using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Common;
using Identity.Domain.Services.User;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Authentication.Commands.Logout;

public class LogoutCommandHandler(
    IUserRepository userRepository,
    IAuthenticationService authenticationService,
    IUnitOfWork unitOfWork,
    ILogger<LogoutCommandHandler> logger)
    : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand cmd, CancellationToken ct)
    {
        var user = await userRepository.GetSinleAsync(new UserWithDeviceAndRefreshTokensSpec(cmd.Id), ct);
        if (user is null)
        {
            logger.LogWarning("User with ID {UserId} not found", cmd.Id);
            return Result.Fail(Errors.General.NotFound(cmd.Id));
        }

        authenticationService.Logout(user, cmd.DeviceId);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
