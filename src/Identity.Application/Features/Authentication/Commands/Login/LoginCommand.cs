using Identity.Application.Abstractions;

namespace Identity.Application.Features.Authentication.Commands.Login;

public sealed record LoginCommand : ICommand<LoginDto>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}