using System.Reflection;
using Cypherly.Identity.Application.Behavior;
using Cypherly.Identity.Application.Features.Authentication.Commands.VerifyNonce;
using Cypherly.Identity.Application.Features.Authentication.Token;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Cypherly.Identity.Application.Extensions;

public static class ApplicationExtensions
{
    public static void AddAuthenticationApplication(this IServiceCollection services, Assembly assembly)
    {
        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddAutoMapper(assembly);
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IVerifyNonceService, VerifyNonceService>();
    }
}