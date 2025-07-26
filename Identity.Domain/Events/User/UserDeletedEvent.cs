using Identity.Domain.Abstractions;

namespace Identity.Domain.Events.User;

public sealed record UserDeletedEvent(Guid UserId, string Email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}