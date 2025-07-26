using Cypherly.Domain.Common;
using Identity.Application.Contracts;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Common;
using Identity.Domain.Services.User;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.User.Commands.Delete;

public class DeleteUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IUserLifeCycleServices userLifeCycleServices,
    ILogger<DeleteUserCommandHandler> logger)
    : ICommandHandler<DeleteUserCommand>
{
    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(request.Id);
            if (user is null)
            {
                logger.LogError("User not found with id {Id} during delete process", request.Id);
                return Result.Fail(Errors.General.NotFound(request.Id));
            }

            if (userLifeCycleServices.IsUserDeleted(user))
            {
                logger.LogError("User with id {Id} is already deleted", request.Id);
                return Result.Fail(Errors.General.UnspecifiedError("User is already marked as deleted"));
            }

            userLifeCycleServices.SoftDelete(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred during delete process for user with id {Id}", request.Id);
            return Result.Fail(Errors.General.UnspecifiedError("An exception occured while attempting to delete the user"));
        }
    }
}