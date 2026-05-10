using System.Reflection;
using Identity.Application.Extensions;
using Identity.Application.Features.Authentication.Token;
using Identity.API.Extensions;
using Identity.Domain.Extensions;
using Identity.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

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

builder.AddLogging();
builder.Services.AddObservability();

builder.Services.AddDomain();

builder.Services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
builder.Services.AddApplication(Assembly.Load("Identity.Application"));

builder.Services.AddInfrastructure(configuration, Assembly.Load("Identity.Infrastructure"));

builder.Services.AddAuthentication(configuration);

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

builder.Services.AddControllers();
builder.Services.AddSwagger();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

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
