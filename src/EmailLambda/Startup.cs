using Amazon.SimpleEmail;
using EmailLambda.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EmailLambda;

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
        services.AddSingleton<IAmazonSimpleEmailService, AmazonSimpleEmailServiceClient>();

        // Register application services
        services.AddSingleton<IEmailService, EmailService>();

        return services.BuildServiceProvider();
    }
}
