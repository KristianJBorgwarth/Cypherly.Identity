using Identity.Application.Abstractions;
using Identity.Domain.Aggregates;

internal sealed class UserByEmailSpec(string email) : Specification<User>(u => u.Email.Address == email);
