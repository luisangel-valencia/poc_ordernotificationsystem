using Amazon.DynamoDBv2;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderLambda.Services;
using OrderLambda.Validators;

namespace OrderLambda;

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
        services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();

        // Register application services
        services.AddSingleton<IOrderService, OrderService>();
        services.AddSingleton<EventPublisher>();
        
        // Register validators
        services.AddSingleton<OrderValidator>();

        return services.BuildServiceProvider();
    }
}
