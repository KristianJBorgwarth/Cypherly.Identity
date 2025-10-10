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
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(request.Id);
            if (user is null)
            {
                logger.LogWarning("User with ID {UserId} not found", request.Id);
                return Result.Fail(Errors.General.NotFound(request.Id));
            }

            authenticationService.Logout(user, request.DeviceId);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occured while logging out user with ID {UserId} and device with ID: {DeviceId}", request.Id, request.DeviceId);
            return Result.Fail(Errors.General.UnspecifiedError("An error occured while logging out user"));
        }
    }
}
