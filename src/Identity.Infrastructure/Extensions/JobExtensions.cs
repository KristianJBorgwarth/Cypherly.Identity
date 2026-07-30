using System.Reflection;
using Identity.Infrastructure.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Identity.Infrastructure.Extensions;

internal static class JobExtensions
{
    private const string ConnectionStringName = "IdentityDbConnectionString";

    internal static void AddJobs(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
    {
        services.AddQuartz(configure =>
        {
            configure.SchedulerId = "AUTO";

            var outboxJobKey = new JobKey($"{nameof(ProcessOutboxMessageJob)}-{assembly.GetName()}");

            configure.AddJob<ProcessOutboxMessageJob>(outboxJobKey)
                .AddTrigger(trigger => trigger.ForJob(outboxJobKey)
                    .WithSimpleSchedule(schedule => schedule.WithIntervalInSeconds(10).RepeatForever()));

            var rotationJobKey = new JobKey($"{nameof(RotateSigningKeysJob)}-{assembly.GetName()}");

            configure.AddJob<RotateSigningKeysJob>(rotationJobKey)
                .AddTrigger(trigger => trigger.ForJob(rotationJobKey)
                    .StartNow()
                    .WithSimpleSchedule(schedule => schedule.WithIntervalInMinutes(1).RepeatForever()));

            configure.UsePersistentStore(store =>
            {
                store.UsePostgres(options =>
                {
                    options.ConnectionString = configuration.GetConnectionString(ConnectionStringName)
                        ?? throw new InvalidOperationException($"Missing {ConnectionStringName} connection string.");
                });

                store.UseSystemTextJsonSerializer();
                store.UseClustering();
            });
        });

        services.AddQuartzHostedService();
    }
}
