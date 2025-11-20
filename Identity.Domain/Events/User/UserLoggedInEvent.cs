using Identity.Domain.Abstractions;

namespace Identity.Domain.Events.User
{
    public sealed record UserLoggedInEvent(Guid UserId, Guid DeviceId, Guid ConnectionId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
