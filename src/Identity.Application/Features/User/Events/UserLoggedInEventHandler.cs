using Cypherly.Message.Contracts.Abstractions;
using Cypherly.Message.Contracts.Messages.User;
using Identity.Application.Abstractions;
using Identity.Domain.Events.User;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.User.Events
{
    public sealed class UserLoggedInEventHandler(
        IProducer<UserLoginMessage> producer,
        ILogger<UserLoggedInEventHandler> logger)
        : IDomainEventHandler<UserLoggedInEvent>
    {
        public async Task Handle(UserLoggedInEvent domainEvent, CancellationToken cancellationToken)
        {
            try
            {
                var userLoginMessage = new UserLoginMessage
                {
                    CorrelationId = Guid.NewGuid(),
                    UserId = domainEvent.UserId,
                    DeviceId = domainEvent.DeviceId,
                    ConnectionId = domainEvent.ConnectionId,
                };

                await producer.PublishMessageAsync(userLoginMessage, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occurred while handling UserLoggedInEvent for UserId: {UserId}, DeviceId: {DeviceId}", domainEvent.UserId, domainEvent.DeviceId);
                throw;
            }
        }
    }
}
