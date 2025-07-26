using Cypherly.Identity.Domain.Abstractions;
using MediatR;

namespace Cypherly.Identity.Application.Abstractions;

public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : IDomainEvent
{

}