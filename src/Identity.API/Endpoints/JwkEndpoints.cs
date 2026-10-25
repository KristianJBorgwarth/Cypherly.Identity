using Identity.API.Common;
using Identity.Application.Features.Authentication.Queries.GetJwks;
using Identity.Application.Features.Authentication.Queries.GetTokenIssuerDirectory;
using Mediator;

internal sealed class JwksEndpoints : IEndpoint
{
    public void MapRoutes(IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("/.well-known")
            .WithTags("JWKS");

        group.MapGet("/jwks.json", async (ISender sender, HttpContext http) =>
            {
                var query = new GetJwksQuery();
                var result = await sender.Send(query);

                if (result.Success is false) return Results.Problem(result.Error.Description);

                http.Response.Headers.CacheControl = "public, max-age=300";

                return Results.Ok(result.Value);
            })
            .WithName("GetJwks")
            .WithDescription("Retrieves the JSON Web Key Set (JWKS) containing the keys used for token validation.")
            .Produces<JwksResponse>(StatusCodes.Status200OK);

        group.MapGet("/private-token-issuer-directory", async (ISender sender, HttpContext http) =>
            {
                var result = await sender.Send(new GetTokenIssuerDirectoryQuery());

                if (result.Success is false) return Results.Problem(result.Error.Description);

                http.Response.Headers.CacheControl = "public, max-age=300";

                return Results.Json(result.Value, contentType: "application/private-token-issuer-directory");
            })
            .WithName("GetTokenIssuerDirectory")
            .WithDescription("Retrieves the Privacy Pass issuer directory (RFC 9578) with the public key tokens are issued under.")
            .Produces<TokenIssuerDirectoryResponse>(StatusCodes.Status200OK, "application/private-token-issuer-directory");

        group.MapGet("/openid-configuration", (IConfiguration config) =>
        {
            var baseUrl = config["Jwt:Issuer"]!.TrimEnd('/');
            return Results.Ok(new
            {
                issuer = config["Jwt:Issuer"],
                jwks_uri = $"{baseUrl}/.well-known/jwks.json",
                id_token_signing_alg_values_supported = new[] { "RS256" },
                response_types_supported = new[] { "token" },
                subject_types_supported = new[] { "public" },
            });
        });
    }
}
