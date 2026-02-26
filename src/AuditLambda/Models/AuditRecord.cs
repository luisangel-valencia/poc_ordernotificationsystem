namespace AuditLambda.Models;

/// <summary>
/// Model for audit records stored in DynamoDB
/// </summary>
public class AuditRecord
{
    public string AuditId { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public OrderDetails OrderDetails { get; set; } = new();
}

/// <summary>
/// Order details snapshot for audit records
/// </summary>
public class OrderDetails
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal TotalAmount { get; set; }
}
