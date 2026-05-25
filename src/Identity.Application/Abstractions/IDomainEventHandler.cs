using Identity.Domain.Abstractions;
using MediatR;

namespace Identity.Application.Abstractions;

public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : IDomainEvent
{

}