using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AuditLambda.Models;
using Microsoft.Extensions.Logging;

namespace AuditLambda.Services;

/// <summary>
/// Service for creating audit records in DynamoDB
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly ILogger<AuditService> _logger;
    private readonly string _tableName;

    public AuditService(IAmazonDynamoDB dynamoDbClient, ILogger<AuditService> logger)
    {
        _dynamoDbClient = dynamoDbClient;
        _logger = logger;
        _tableName = Environment.GetEnvironmentVariable("AUDIT_TABLE_NAME") 
            ?? throw new InvalidOperationException("AUDIT_TABLE_NAME environment variable is not set");
    }

    public async Task CreateAuditRecordAsync(OrderEvent orderEvent)
    {
        try
        {
            // Generate unique audit ID
            var auditId = Guid.NewGuid().ToString();
            
            // Create timestamp in ISO8601 format
            var timestamp = DateTime.UtcNow.ToString("o");

            _logger.LogInformation("Creating audit record for OrderId: {OrderId}, AuditId: {AuditId}", 
                orderEvent.OrderId, auditId);

            // Create audit record
            var auditRecord = new AuditRecord
            {
                AuditId = auditId,
                Timestamp = timestamp,
                OrderId = orderEvent.OrderId,
                EventType = "ORDER_CREATED",
                OrderDetails = new OrderDetails
                {
                    CustomerId = orderEvent.CustomerId,
                    CustomerName = orderEvent.CustomerName,
                    CustomerEmail = orderEvent.CustomerEmail,
                    ItemCount = orderEvent.Items.Count,
                    TotalAmount = orderEvent.TotalAmount
                }
            };

            // Prepare DynamoDB item
            var item = new Dictionary<string, AttributeValue>
            {
                ["AuditId"] = new AttributeValue { S = auditRecord.AuditId },
                ["Timestamp"] = new AttributeValue { S = auditRecord.Timestamp },
                ["OrderId"] = new AttributeValue { S = auditRecord.OrderId },
                ["EventType"] = new AttributeValue { S = auditRecord.EventType },
                ["OrderDetails"] = new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {
                        ["CustomerId"] = new AttributeValue { S = auditRecord.OrderDetails.CustomerId },
                        ["CustomerName"] = new AttributeValue { S = auditRecord.OrderDetails.CustomerName },
                        ["CustomerEmail"] = new AttributeValue { S = auditRecord.OrderDetails.CustomerEmail },
                        ["ItemCount"] = new AttributeValue { N = auditRecord.OrderDetails.ItemCount.ToString() },
                        ["TotalAmount"] = new AttributeValue { N = auditRecord.OrderDetails.TotalAmount.ToString("F2") }
                    }
                }
            };

            // Write to DynamoDB
            var request = new PutItemRequest
            {
                TableName = _tableName,
                Item = item
            };

            await _dynamoDbClient.PutItemAsync(request);

            _logger.LogInformation("Successfully created audit record for OrderId: {OrderId}, AuditId: {AuditId}", 
                orderEvent.OrderId, auditId);
        }
        catch (AmazonDynamoDBException ex)
        {
            _logger.LogError(ex, "DynamoDB error creating audit record for OrderId: {OrderId}", 
                orderEvent.OrderId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating audit record for OrderId: {OrderId}", 
                orderEvent.OrderId);
            throw;
        }
    }
}
