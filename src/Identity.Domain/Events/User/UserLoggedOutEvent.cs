using Identity.Domain.Abstractions;

namespace Identity.Domain.Events.User;

public sealed record UserLoggedOutEvent(Guid UserId, Guid DeviceId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
