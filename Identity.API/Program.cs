using System.Reflection;
using System.Text;
using Identity.Application.Extensions;
using Identity.Application.Features.Authentication.Token;
using Identity.API.Extensions;
using Identity.API.Filters;
using Identity.Domain.Extensions;
using Identity.Infrastructure.Extensions;
using Microsoft.IdentityModel.Tokens;
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

#region Authenticaion & Authorization

var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        if (jwtSettings is null)
            throw new NotImplementedException("MISSING JWT SETTINGS");

        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer ?? throw new NotImplementedException($"MISSING VALUE IN JWT SETTINGS {jwtSettings.Issuer}"),
            ValidAudience = jwtSettings.Audience ?? throw new NotImplementedException($"MISSING VALUE IN JWT SETTINGS {jwtSettings.Audience}"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IValidateUserIdFilter, ValidateUserIdIdFilter>();

#endregion

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cypherly.Identity.API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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