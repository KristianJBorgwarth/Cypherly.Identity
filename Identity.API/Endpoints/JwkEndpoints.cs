using Identity.API.Common;
using Identity.Application.Features.Authentication.Queries.GetJwks;
using MediatR;

internal sealed class JwksEndpoints : IEndpoint
{
    public void MapRoutes(IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("/.well-known")
            .WithTags("JWKS");

        group.MapGet("/jwks.json", async (ISender sender) =>
            {
                var query = new GetJwksQuery();
                var result = await sender.Send(query);
                return result.Success
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Error.Message);
            })
            .WithName("GetJwks")
            .WithDescription("Retrieves the JSON Web Key Set (JWKS) containing the keys used for token validation.")
            .Produces<JwksResponse>(StatusCodes.Status200OK);
    }
}
