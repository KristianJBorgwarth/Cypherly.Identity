using System.Text.Json.Serialization;

namespace Identity.Application.Features.Authentication.Queries.GetTokenIssuerDirectory;

public sealed record TokenIssuerDirectoryResponse
{
    [JsonPropertyName("issuer-request-uri")]
    public required string IssuerRequestUri { get; init; }

    [JsonPropertyName("token-keys")]
    public required IReadOnlyList<TokenKeyDto> TokenKeys { get; init; }
}

public sealed record TokenKeyDto
{
    [JsonPropertyName("token-type")]
    public required ushort TokenType { get; init; }

    [JsonPropertyName("token-key")]
    public required string TokenKey { get; init; }
}
