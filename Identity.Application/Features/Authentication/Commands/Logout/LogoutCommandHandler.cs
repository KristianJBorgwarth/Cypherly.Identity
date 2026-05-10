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
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("User with ID {UserId} not found", request.Id);
            return Result.Fail(Errors.General.NotFound(request.Id));
        }

        authenticationService.Logout(user, request.DeviceId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
