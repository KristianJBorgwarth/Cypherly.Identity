using MediatR;

namespace Identity.Application.Features.Authentication.Queries.GetJwks;

public sealed record GetJwksQuery : IRequest<IReadOnlyList<JwksDto>>;
