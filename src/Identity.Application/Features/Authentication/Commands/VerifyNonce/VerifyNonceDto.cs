namespace Identity.Application.Features.Authentication.Commands.VerifyNonce;

#pragma warning disable IDE0290 // Use primary constructor
public sealed class VerifyNonceDto
{
    public string Jwt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public IReadOnlyList<string> TokenResponses { get; private set; }

    public VerifyNonceDto(string jwt, DateTime expiresAt, IReadOnlyList<string>? tokenResponses = null)
    {
        Jwt = jwt;
        ExpiresAt = expiresAt;
        TokenResponses = tokenResponses ?? [];
    }
}
