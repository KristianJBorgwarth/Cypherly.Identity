using Identity.Domain.Abstractions;

namespace Identity.Domain.Events.User;

public sealed record UserCreatedEvent(Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}