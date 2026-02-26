using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderApi.Models;
using OrderApi.Services;
using OrderApi.Validators;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace OrderApi;

public class Function
{
    private readonly IOrderService _orderService;
    private readonly EventPublisher _eventPublisher;
    private readonly OrderValidator _validator;
    private readonly ILogger<Function> _logger;
    private static readonly Lazy<IServiceProvider> _serviceProvider = new(() => Startup.ConfigureServices());

    public Function() : this(
        _serviceProvider.Value.GetRequiredService<IOrderService>(),
        _serviceProvider.Value.GetRequiredService<EventPublisher>(),
        _serviceProvider.Value.GetRequiredService<OrderValidator>(),
        _serviceProvider.Value.GetRequiredService<ILogger<Function>>())
    {
    }

    // Constructor for testing with dependency injection
    public Function(IOrderService orderService, EventPublisher eventPublisher, OrderValidator validator, ILogger<Function> logger)
    {
        _orderService = orderService;
        _eventPublisher = eventPublisher;
        _validator = validator;
        _logger = logger;
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var requestId = context.AwsRequestId;
        
        _logger.LogInformation("Processing order request. RequestId: {RequestId}", requestId);

        try
        {
            // Deserialize request body to Order object
            if (string.IsNullOrEmpty(request.Body))
            {
                _logger.LogWarning("Empty request body. RequestId: {RequestId}", requestId);
                return CreateErrorResponse(400, "Request body is required", requestId);
            }

            Order order;
            try
            {
                order = JsonSerializer.Deserialize<Order>(request.Body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new JsonException("Failed to deserialize order");
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid JSON in request body. RequestId: {RequestId}", requestId);
                return CreateErrorResponse(400, "Invalid JSON format", requestId);
            }

            // Validate order using OrderValidator
            var validationResult = await _validator.ValidateAsync(order);
            
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Order validation failed. RequestId: {RequestId}, Errors: {Errors}", 
                    requestId, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

                var validationErrors = validationResult.Errors.Select(e => new ValidationError
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                }).ToList();

                var errorResponse = new ApiResponse
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = validationErrors
                };

                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonSerializer.Serialize(errorResponse),
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" },
                        { "X-Request-Id", requestId }
                    }
                };
            }

            // Save order to DynamoDB
            try
            {
                await _orderService.SaveOrderAsync(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save order. RequestId: {RequestId}", requestId);
                return CreateErrorResponse(500, "Failed to save order", requestId);
            }

            // Publish order event to SNS
            try
            {
                await _eventPublisher.PublishOrderEventAsync(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish order event. RequestId: {RequestId}, OrderId: {OrderId}", 
                    requestId, order.OrderId);
                return CreateErrorResponse(500, "Order saved but failed to publish event", requestId);
            }

            // Return success response
            _logger.LogInformation("Order processed successfully. RequestId: {RequestId}, OrderId: {OrderId}", 
                requestId, order.OrderId);

            var successResponse = new ApiResponse
            {
                Success = true,
                Message = "Order received successfully",
                Data = new OrderConfirmation
                {
                    OrderId = order.OrderId!,
                    Message = "Order received successfully",
                    CreatedAt = order.CreatedAt.ToString("o")
                }
            };

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(successResponse),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "X-Request-Id", requestId }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing order. RequestId: {RequestId}, Error: {ErrorMessage}", 
                requestId, ex.Message);
            return CreateErrorResponse(500, "Internal server error", requestId);
        }
    }

    private APIGatewayProxyResponse CreateErrorResponse(int statusCode, string message, string requestId)
    {
        var errorResponse = new ApiResponse
        {
            Success = false,
            Message = message
        };

        return new APIGatewayProxyResponse
        {
            StatusCode = statusCode,
            Body = JsonSerializer.Serialize(errorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "X-Request-Id", requestId }
            }
        };
    }
}
