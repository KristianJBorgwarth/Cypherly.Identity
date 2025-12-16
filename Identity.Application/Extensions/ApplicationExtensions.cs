using System.Reflection;
using FluentValidation;
using Identity.Application.Behavior;
using Identity.Application.Features.Authentication.Commands.VerifyNonce;
using Identity.Application.Features.Authentication.Token;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Application.Extensions;

public static class ApplicationExtensions
{
    public static void AddApplication(this IServiceCollection services, Assembly assembly)
    {
        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionBehavior<,>));
        services.AddAutoMapper(assembly);
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IVerifyNonceService, VerifyNonceService>();
    }
}
