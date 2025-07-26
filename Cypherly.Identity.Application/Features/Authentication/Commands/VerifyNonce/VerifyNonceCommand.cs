using Cypherly.Identity.Application.Abstractions;

namespace Cypherly.Identity.Application.Features.Authentication.Commands.VerifyNonce;

public class VerifyNonceCommand : ICommand<VerifyNonceDto>
{
    public Guid UserId { get; init; }
    public Guid NonceId { get; init; }
    public Guid DeviceId { get; init; }
    public required string Nonce { get; init; }
}