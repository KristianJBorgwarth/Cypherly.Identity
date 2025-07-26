using MediatR;

namespace Cypherly.Identity.Domain.Abstractions;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}