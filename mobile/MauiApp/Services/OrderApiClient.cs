using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MauiApp.Models;

namespace MauiApp.Services;

public class OrderApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiEndpoint;
    private const int MaxRetries = 2;
    private const int TimeoutSeconds = 30;

    public OrderApiClient(string apiEndpoint)
    {
        _apiEndpoint = apiEndpoint;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(TimeoutSeconds)
        };
    }

    public async Task<OrderApiResult> SubmitOrderAsync(Order order)
    {
        int retryCount = 0;
        Exception? lastException = null;

        while (retryCount <= MaxRetries)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_apiEndpoint}/order", order);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var successResponse = await response.Content.ReadFromJsonAsync<OrderResponse>();
                    return new OrderApiResult
                    {
                        Success = true,
                        OrderId = successResponse?.OrderId,
                        Message = successResponse?.Message ?? "Order submitted successfully"
                    };
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    return new OrderApiResult
                    {
                        Success = false,
                        Message = "Validation failed",
                        ValidationErrors = errorResponse?.Errors?.Select(e => $"{e.Field}: {e.Message}").ToList() ?? new List<string>()
                    };
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    
                    // For 500 errors, retry with exponential backoff
                    if (retryCount < MaxRetries)
                    {
                        retryCount++;
                        var delayMs = (int)Math.Pow(2, retryCount) * 1000; // Exponential backoff: 2s, 4s
                        await Task.Delay(delayMs);
                        continue;
                    }
                    
                    return new OrderApiResult
                    {
                        Success = false,
                        Message = errorResponse?.Message ?? "Server error occurred. Please try again later."
                    };
                }
                else
                {
                    return new OrderApiResult
                    {
                        Success = false,
                        Message = $"Unexpected response: {response.StatusCode}"
                    };
                }
            }
            catch (TaskCanceledException)
            {
                // Timeout occurred
                if (retryCount < MaxRetries)
                {
                    retryCount++;
                    var delayMs = (int)Math.Pow(2, retryCount) * 1000;
                    await Task.Delay(delayMs);
                    continue;
                }
                
                return new OrderApiResult
                {
                    Success = false,
                    Message = "Request timed out. Please check your connection and try again."
                };
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                
                // Network error - retry with exponential backoff
                if (retryCount < MaxRetries)
                {
                    retryCount++;
                    var delayMs = (int)Math.Pow(2, retryCount) * 1000;
                    await Task.Delay(delayMs);
                    continue;
                }
            }
            catch (Exception ex)
            {
                return new OrderApiResult
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        return new OrderApiResult
        {
            Success = false,
            Message = $"Failed after {MaxRetries} retries. {lastException?.Message}"
        };
    }
}

public class OrderApiResult
{
    public bool Success { get; set; }
    public string? OrderId { get; set; }
    public string? Message { get; set; }
    public List<string>? ValidationErrors { get; set; }
}
