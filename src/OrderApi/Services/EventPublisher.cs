using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using OrderApi.Models;
using System.Text.Json;

namespace OrderApi.Services;

public class EventPublisher
{
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly ILogger<EventPublisher> _logger;
    private readonly string _topicArn;

    public EventPublisher(IAmazonSimpleNotificationService snsClient, ILogger<EventPublisher> logger)
    {
        _snsClient = snsClient;
        _logger = logger;
        _topicArn = Environment.GetEnvironmentVariable("ORDER_TOPIC_ARN") 
            ?? throw new InvalidOperationException("ORDER_TOPIC_ARN environment variable is not set");
    }

    public async Task PublishOrderEventAsync(Order order)
    {
        try
        {
            var orderEvent = new
            {
                orderId = order.OrderId,
                customerId = order.CustomerId,
                customerName = order.CustomerName,
                customerEmail = order.CustomerEmail,
                items = order.Items,
                totalAmount = order.TotalAmount,
                createdAt = order.CreatedAt.ToString("o")
            };

            var message = JsonSerializer.Serialize(orderEvent);

            var request = new PublishRequest
            {
                TopicArn = _topicArn,
                Message = message,
                Subject = $"Order Created: {order.OrderId}"
            };

            _logger.LogInformation("Publishing order event for {OrderId} to SNS topic {TopicArn}", 
                order.OrderId, _topicArn);

            var response = await _snsClient.PublishAsync(request);

            _logger.LogInformation("Successfully published order event for {OrderId}, MessageId: {MessageId}", 
                order.OrderId, response.MessageId);
        }
        catch (AmazonSimpleNotificationServiceException ex)
        {
            _logger.LogError(ex, "SNS error while publishing order event for {OrderId}: {ErrorMessage}", 
                order.OrderId, ex.Message);
            throw new InvalidOperationException("Failed to publish order event", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while publishing order event for {OrderId}: {ErrorMessage}", 
                order.OrderId, ex.Message);
            throw;
        }
    }
}
