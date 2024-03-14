using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Polly;
using Polly.Retry;

namespace FlorianAlbert.FinanceObserver.Server.Startup.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseMigrations(this IApplicationBuilder app)
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<NpgsqlException>(),
                MaxRetryAttempts = 5,
                DelayGenerator = static args =>
                {
                    var delayInSeconds = Math.Pow(2, args.AttemptNumber);

                    return ValueTask.FromResult(TimeSpan.FromSeconds(delayInSeconds) as TimeSpan?);
                }
            })
            .Build();

        using var scope = app.ApplicationServices.CreateScope();
        
        using var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
        pipeline.Execute(dbContext.Database.Migrate);

        return app;
    }
}