using Amazon.DynamoDBv2;
using AuditLambda.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuditLambda;

/// <summary>
/// Configures dependency injection for the Lambda function
/// </summary>
public class Startup
{
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Configure logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
        });

        // Register AWS services
        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();

        // Register application services
        services.AddSingleton<IAuditService, AuditService>();

        return services.BuildServiceProvider();
    }
}
