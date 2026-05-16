using Identity.Application.Abstractions;

namespace Identity.Application.Features.Authentication.Queries.GetJwks;

public sealed record GetJwksQuery : IQuery<JwksResponse>;
