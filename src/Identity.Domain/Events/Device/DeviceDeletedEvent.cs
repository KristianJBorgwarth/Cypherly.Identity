using Identity.Domain.Abstractions;

namespace Identity.Domain.Events.Device;

public sealed record DeviceDeletedEvent(Guid UserId, Guid DeviceId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
