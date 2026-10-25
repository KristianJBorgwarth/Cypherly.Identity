using Identity.Application.Abstractions;

namespace Identity.Application.Features.Authentication.Queries.GetTokenIssuerDirectory;

public sealed record GetTokenIssuerDirectoryQuery : IQuery<TokenIssuerDirectoryResponse>;
