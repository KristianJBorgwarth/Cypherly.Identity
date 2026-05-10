using Cypherly.Message.Contracts.Abstractions;
using Cypherly.Message.Contracts.Messages.User;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Events.User;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.User.Events;

public sealed class UserLoggedOutEventHandler(
    IUserRepository userRepository,
    IProducer<UserLogoutMessage> userLogoutProducer,
    ILogger<UserLoggedOutEventHandler> logger)
    : IDomainEventHandler<UserLoggedOutEvent>
{
    public async Task Handle(UserLoggedOutEvent notification, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (user is null)
        {
            logger.LogError("User with id {UserId} not found", notification.UserId);
            throw new InvalidOperationException("User not found");
        }

        var device = user.GetDevice(notification.DeviceId, true);

        var userLogoutMessage = new UserLogoutMessage
        {
            CorrelationId = Guid.NewGuid(),
            UserId = notification.UserId,
            DeviceId = device.Id,
            ConnectionId = device.ConnectionId,
        };

        await userLogoutProducer.PublishMessageAsync(userLogoutMessage, cancellationToken);
    }
}
