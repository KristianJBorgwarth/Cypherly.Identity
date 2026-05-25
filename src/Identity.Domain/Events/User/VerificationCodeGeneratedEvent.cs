using Identity.Domain.Abstractions;
using Identity.Domain.Enums;

namespace Identity.Domain.Events.User;

public sealed record VerificationCodeGeneratedEvent(Guid UserId, UserVerificationCodeType CodeType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}