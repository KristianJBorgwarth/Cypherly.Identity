using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Aggregates;
using Identity.Domain.Common;
using Identity.Domain.Services.User;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.User.Commands.Delete;

public class DeleteUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IUserLifeCycleService userLifeCycleServices,
    ILogger<DeleteUserCommandHandler> logger)
    : ICommandHandler<DeleteUserCommand>
{
    public async Task<Result> Handle(DeleteUserCommand cmd, CancellationToken ct)
    {
        var user = await userRepository.GetSinleAsync(new UserSpec(cmd.Id), ct);
        if (user is null)
        {
            logger.LogError("User not found with id {Id} during delete process", cmd.Id);
            return Result.Fail(Error.NotFound<User>(cmd.Id.ToString()));
        }

        if (userLifeCycleServices.IsUserDeleted(user))
        {
            logger.LogError("User with id {Id} is already deleted", cmd.Id);
            return Result.Fail(Error.BadRequest("user.already.deleted", "User is already marked as deleted"));
        }

        userLifeCycleServices.SoftDelete(user);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
