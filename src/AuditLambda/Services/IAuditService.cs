using AuditLambda.Models;

namespace AuditLambda.Services;

/// <summary>
/// Interface for audit service operations
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Creates an audit record in DynamoDB for an order event
    /// </summary>
    /// <param name="orderEvent">The order event to audit</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task CreateAuditRecordAsync(OrderEvent orderEvent);
}
