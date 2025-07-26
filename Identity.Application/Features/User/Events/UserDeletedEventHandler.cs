using Identity.Domain.Events.User;
using Cypherly.Message.Contracts.Abstractions;
using Cypherly.Message.Contracts.Messages.User;
using Identity.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.User.Events;

public class UserDeletedEventHandler(
    IProducer<UserDeletedMessage> producer,
    ILogger<UserDeletedEventHandler> logger)
    : IDomainEventHandler<UserDeletedEvent>
{
    public async Task Handle(UserDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("User with id {UserId} and email {Email} has been deleted", notification.UserId, notification.Email);
        
        var message = new UserDeletedMessage
        {
            CorrelationId = Guid.NewGuid(),
            UserId = notification.UserId,
            Email = notification.Email,
            CausationId = null
        };
        
        await producer.PublishMessageAsync(message, cancellationToken);
    }
}