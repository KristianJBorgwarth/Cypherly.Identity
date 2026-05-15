using Identity.Domain.Services.User;
using Cypherly.Message.Contracts.Enums;
using Cypherly.Message.Contracts.Messages.User;
using Identity.Application.Contracts.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.User.Consumers;

public class RollbackUserDeleteConsumer(
    IUserRepository userRepository,
    IUserLifeCycleService userLifeCycleServices,
    IUnitOfWork unitOfWork,
    ILogger<RollbackUserDeleteConsumer> logger)
    : IConsumer<UserDeleteFailedMessage>
{

    public async Task Consume(ConsumeContext<UserDeleteFailedMessage> ctx)
    {
        try
        {
            var message = ctx.Message;

            if (!message.ContainsService(ServiceType.AuthenticationService)) return;

            var user = await userRepository.GetSinleAsync(new UserSpec(message.UserId), ctx.CancellationToken);
            if (user is null)
            {
                logger.LogError("User with id {UserId} not found", message.UserId);
                throw new KeyNotFoundException($"User with id {message.UserId} not found.");
            }

            logger.LogInformation("Reverting soft delete for user with id {UserId}", message.UserId);
            userLifeCycleServices.RevertSoftDelete(user);
            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming UserDeleteFailed message");
            throw;
        }
    }
}
