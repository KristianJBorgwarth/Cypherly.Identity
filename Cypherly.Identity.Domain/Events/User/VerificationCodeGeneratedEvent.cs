using Cypherly.Identity.Domain.Abstractions;
using Cypherly.Identity.Domain.Enums;

namespace Cypherly.Identity.Domain.Events.User;

public sealed record VerificationCodeGeneratedEvent(Guid UserId, UserVerificationCodeType CodeType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}