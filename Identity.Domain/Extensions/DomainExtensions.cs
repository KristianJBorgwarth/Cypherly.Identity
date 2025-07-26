using Identity.Domain.Services.User;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Domain.Extensions;

public static class DomainExtensions
{
    public static void AddAuthenticationDomain(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IUserLifeCycleServices, UserLifeCycleServices>();
        serviceCollection.AddScoped<IVerificationCodeService, VerificationCodeService>();
        serviceCollection.AddScoped<IAuthenticationService, AuthenticationService>();
        serviceCollection.AddScoped<IDeviceService, DeviceService>();
    }
}