using Serilog;
using Serilog.Enrichers.Span;

namespace Identity.API.Extensions;

public static class LoggerBuilderExtensions
{
    public static void AddSerilogger(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Host.UseSerilog((ctx, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(ctx.Configuration)
                .ReadFrom.Services(services)
                .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName)
                .Enrich.WithProperty("ServiceName", "cypherly.identity")
                .Enrich.FromLogContext()
                .Enrich.WithSpan();
        });

        Serilog.Debugging.SelfLog.Enable(Console.Error);
    }
}
