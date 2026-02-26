using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using OrderApi.Models;
using System.Text.Json;

namespace OrderApi.Services;

public class OrderService : IOrderService
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly ILogger<OrderService> _logger;
    private readonly string _tableName;

    public OrderService(IAmazonDynamoDB dynamoDbClient, ILogger<OrderService> logger)
    {
        _dynamoDbClient = dynamoDbClient;
        _logger = logger;
        _tableName = Environment.GetEnvironmentVariable("ORDER_TABLE_NAME") 
            ?? throw new InvalidOperationException("ORDER_TABLE_NAME environment variable is not set");
    }

    public async Task SaveOrderAsync(Order order)
    {
        try
        {
            // Generate unique OrderId
            order.OrderId = Guid.NewGuid().ToString();
            
            // Add CreatedAt timestamp in ISO8601 format
            order.CreatedAt = DateTime.UtcNow;

            // Calculate subtotals and total amount
            foreach (var orderItem in order.Items)
            {
                orderItem.Subtotal = orderItem.Quantity * orderItem.Price;
            }
            order.TotalAmount = order.Items.Sum(i => i.Subtotal);

            var item = new Dictionary<string, AttributeValue>
            {
                ["OrderId"] = new AttributeValue { S = order.OrderId },
                ["CustomerId"] = new AttributeValue { S = order.CustomerId ?? string.Empty },
                ["CustomerName"] = new AttributeValue { S = order.CustomerName },
                ["CustomerEmail"] = new AttributeValue { S = order.CustomerEmail },
                ["Items"] = new AttributeValue { S = JsonSerializer.Serialize(order.Items) },
                ["TotalAmount"] = new AttributeValue { N = order.TotalAmount.ToString("F2") },
                ["CreatedAt"] = new AttributeValue { S = order.CreatedAt.ToString("o") }
            };

            var request = new PutItemRequest
            {
                TableName = _tableName,
                Item = item
            };

            _logger.LogInformation("Saving order {OrderId} to DynamoDB table {TableName}", 
                order.OrderId, _tableName);

            await _dynamoDbClient.PutItemAsync(request);

            _logger.LogInformation("Successfully saved order {OrderId}", order.OrderId);
        }
        catch (AmazonDynamoDBException ex)
        {
            _logger.LogError(ex, "DynamoDB error while saving order: {ErrorMessage}", ex.Message);
            throw new InvalidOperationException("Failed to save order to database", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while saving order: {ErrorMessage}", ex.Message);
            throw;
        }
    }
}
