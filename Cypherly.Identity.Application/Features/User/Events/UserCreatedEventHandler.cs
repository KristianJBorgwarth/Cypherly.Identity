using Cypherly.Identity.Application.Abstractions;
using Cypherly.Identity.Application.Contracts.Repository;
using Cypherly.Identity.Domain.Enums;
using Cypherly.Identity.Domain.Events.User;
using Cypherly.Message.Contracts.Abstractions;
using Cypherly.Message.Contracts.Messages.Email;
using Microsoft.Extensions.Logging;

namespace Cypherly.Identity.Application.Features.User.Events;

public class UserCreatedEventHandler(
    IUserRepository userRepository,
    IProducer<SendEmailMessage> emailProducer,
    ILogger<UserCreatedEventHandler> logger)
    : IDomainEventHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(notification.UserId);
        if (user is null)
        {
            logger.LogError("User with id {UserId} not found", notification.UserId);
            throw new InvalidOperationException("User not found");
        }

        var verificationCode = user.GetActiveVerificationCode(UserVerificationCodeType.EmailVerification);
        if (verificationCode is null)
        {
            logger.LogError("Verification code for user with id {UserId} not found", notification.UserId);
            throw new InvalidOperationException("Verification code not found");
        }

        var emailMessage = new SendEmailMessage
        {
            CorrelationId = Guid.NewGuid(),
            To = user.Email.Address,
            Subject = "Welcome to Cypherly",
            Body = "Welcome to Cypherly! Here is your verification code: " + verificationCode.Code.Value,
            CausationId = null,
        };
        
        await emailProducer.PublishMessageAsync(emailMessage, cancellationToken);
    }
}