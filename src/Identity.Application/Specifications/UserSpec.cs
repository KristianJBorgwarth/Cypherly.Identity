using Identity.Application.Abstractions;
using Identity.Domain.Aggregates;

internal sealed class UserSpec(Guid Id) : Specification<User>(u => u.Id == Id);
