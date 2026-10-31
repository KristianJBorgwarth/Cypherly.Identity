using System.Buffers.Text;
using Cendrite.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Interfaces;
using PrivacyPass.Tokens;

namespace Identity.Application.Features.Authentication.Queries.GetTokenIssuerDirectory;

public sealed class GetTokenIssuerDirectoryQueryHandler(ITokenIssuer tokenIssuer)
    : IQueryHandler<GetTokenIssuerDirectoryQuery, TokenIssuerDirectoryResponse>
{
    private const string IssuerRequestUri = "/api/authentication/verify-nonce";

    public async ValueTask<Result<TokenIssuerDirectoryResponse>> Handle(GetTokenIssuerDirectoryQuery q, CancellationToken ct)
    {
        var publicKey = await tokenIssuer.GetPublicKeyAsync(ct);

        return new TokenIssuerDirectoryResponse
        {
            IssuerRequestUri = IssuerRequestUri,
            TokenKeys = publicKey is null
                ? []
                : [new TokenKeyDto { TokenType = TokenType.BlindRsa2048, TokenKey = Base64Url.EncodeToString(publicKey) }],
        };
    }
}
