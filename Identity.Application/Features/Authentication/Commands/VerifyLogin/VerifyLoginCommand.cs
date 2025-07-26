using Identity.Application.Abstractions;

namespace Identity.Application.Features.Authentication.Commands.VerifyLogin;

public sealed record VerifyLoginCommand : ICommand<VerifyLoginDto>
{
    public required Guid UserId { get; init; }
    public required string LoginVerificationCode { get; init; }
}