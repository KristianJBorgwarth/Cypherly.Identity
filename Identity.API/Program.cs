using System.Reflection;
using Identity.Application.Extensions;
using Identity.Application.Features.Authentication.Token;
using Identity.API.Extensions;
using Identity.Domain.Extensions;
using Identity.Infrastructure.Extensions;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

#region Extensions

var env = builder.Environment;

var configuration = builder.Configuration;
configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables();

if (env.IsDevelopment())
{
    configuration.AddJsonFile($"appsettings.{Environments.Development}.json", true, true);
    configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
}

#endregion

#region Logger

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddObservability(configuration);

Serilog.Debugging.SelfLog.Enable(Console.Error);

#endregion

#region Domain Layer

builder.Services.AddAuthenticationDomain();

#endregion

#region Application Layer

builder.Services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
builder.Services.AddAuthenticationApplication(Assembly.Load("Identity.Application"));

#endregion

#region Infrastructure Layer

builder.Services.AddInfrastructure(configuration, Assembly.Load("Identity.Infrastructure"));

#endregion

builder.Services.AddAuthentication(configuration);

#region CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowElectron", policy =>
    {
        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();
        policy.WithOrigins(allowedOrigins!)
            .AllowAnyMethod()
            .AllowCredentials()
            .AllowAnyHeader();
    });
});

#endregion

builder.Services.AddControllers();
builder.Services.AddSwagger();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPrometheusScrapingEndpoint();
app.UseSerilogRequestLogging();

if (app.Environment.IsProduction())
{
    app.Services.ApplyPendingMigrations();
}

app.UseHttpsRedirection();

app.UseCors("AllowElectron");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }