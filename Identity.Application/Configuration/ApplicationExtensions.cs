using System.Reflection;
using FluentValidation;
using Identity.Application.Behavior;
using Identity.Application.Features.Authentication.Commands.VerifyNonce;
using Identity.Application.Interfaces;
using Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Application.Configuration;

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
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IVerifyNonceService, VerifyNonceService>();
    }
}
