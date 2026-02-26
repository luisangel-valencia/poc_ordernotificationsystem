using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using AuditLambda.Models;
using AuditLambda.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AuditLambda;

/// <summary>
/// Lambda function handler for processing order audit records from SQS
/// </summary>
public class Function
{
    private readonly IAuditService _auditService;
    private readonly ILogger<Function> _logger;
    private static readonly Lazy<IServiceProvider> _serviceProvider = new(() => Startup.ConfigureServices());

    public Function() : this(
        _serviceProvider.Value.GetRequiredService<IAuditService>(),
        _serviceProvider.Value.GetRequiredService<ILogger<Function>>())
    {
    }

    // Constructor for dependency injection (testing)
    public Function(IAuditService auditService, ILogger<Function> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Lambda function handler for processing SQS events containing order audit data
    /// </summary>
    /// <param name="sqsEvent">The SQS event containing order messages</param>
    /// <param name="context">Lambda execution context</param>
    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        _logger.LogInformation("Processing {Count} messages from SQS", sqsEvent.Records.Count);

        foreach (var record in sqsEvent.Records)
        {
            try
            {
                _logger.LogInformation("Processing message {MessageId}", record.MessageId);

                // Parse order event from SQS message body
                var orderEvent = ParseOrderEvent(record.Body);

                if (orderEvent == null)
                {
                    _logger.LogError("Failed to parse order event from message {MessageId}", record.MessageId);
                    throw new InvalidOperationException($"Invalid message format for message {record.MessageId}");
                }

                _logger.LogInformation("Parsed order event for OrderId: {OrderId}", orderEvent.OrderId);

                // Create audit record
                await _auditService.CreateAuditRecordAsync(orderEvent);

                _logger.LogInformation("Successfully processed message {MessageId} for OrderId: {OrderId}", 
                    record.MessageId, orderEvent.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {MessageId}. Message will be retried.", 
                    record.MessageId);
                
                // Throw exception to trigger SQS retry mechanism
                throw;
            }
        }

        _logger.LogInformation("Completed processing all messages");
    }

    private OrderEvent? ParseOrderEvent(string messageBody)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var orderEvent = JsonSerializer.Deserialize<OrderEvent>(messageBody, options);
            
            if (orderEvent == null)
            {
                _logger.LogError("Deserialized order event is null");
                return null;
            }

            // Validate required fields
            if (string.IsNullOrEmpty(orderEvent.OrderId))
            {
                _logger.LogError("Order event missing required field: OrderId");
                return null;
            }

            return orderEvent;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize message body: {MessageBody}", messageBody);
            return null;
        }
    }
}
