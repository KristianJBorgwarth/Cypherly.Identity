using Cypherly.Identity.Domain.Abstractions;

namespace Cypherly.Identity.Domain.Events.User;

public sealed record DeviceCreatedEvent(Guid UserId, string DeviceVerificationCode) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}