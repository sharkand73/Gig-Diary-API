using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using ApiLambda.Controllers;
using ApiLambda.Repositories;
using ApiLambda.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ApiLambda.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddApiLambda(this IServiceCollection services)
    {
        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient());
        services.AddSingleton<IDynamoDBContext>(sp => new DynamoDBContextBuilder()
            .WithDynamoDBClient(sp.GetRequiredService<IAmazonDynamoDB>)
            .Build());
        services.AddSingleton<ISecretStore, SecretStore>();
        services.AddSingleton<IGigCalendarFactory, GoogleCalendarFactory>();
        services.AddSingleton<IGigRepository, GigRepository>();
        services.AddSingleton<IGigService, GigService>();
        services.AddSingleton<IGigStatsService, GigStatsService>();
        services.AddSingleton<GigController>();
        services.AddSingleton<StatsController>();
    }
}