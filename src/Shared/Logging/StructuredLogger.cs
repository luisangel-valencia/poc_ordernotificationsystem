using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Shared.Logging;

/// <summary>
/// Provides structured logging with consistent format across all Lambda functions
/// </summary>
public class StructuredLogger
{
    private readonly ILogger _logger;
    private readonly string _component;

    public StructuredLogger(ILogger logger, string component)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _component = component ?? throw new ArgumentNullException(nameof(component));
    }

    /// <summary>
    /// Logs an informational message with structured data
    /// </summary>
    public void LogInfo(string message, object? data = null, string? requestId = null)
    {
        var logEntry = CreateLogEntry(LogLevel.Information, message, data, requestId);
        _logger.LogInformation("{LogEntry}", JsonSerializer.Serialize(logEntry));
    }

    /// <summary>
    /// Logs a warning message with structured data
    /// </summary>
    public void LogWarning(string message, object? data = null, string? requestId = null)
    {
        var logEntry = CreateLogEntry(LogLevel.Warning, message, data, requestId);
        _logger.LogWarning("{LogEntry}", JsonSerializer.Serialize(logEntry));
    }

    /// <summary>
    /// Logs an error message with structured data and optional exception
    /// </summary>
    public void LogError(string message, Exception? exception = null, object? data = null, string? requestId = null)
    {
        var logEntry = CreateLogEntry(LogLevel.Error, message, data, requestId, exception);
        _logger.LogError("{LogEntry}", JsonSerializer.Serialize(logEntry));
    }

    private Dictionary<string, object?> CreateLogEntry(
        LogLevel level,
        string message,
        object? data,
        string? requestId,
        Exception? exception = null)
    {
        var entry = new Dictionary<string, object?>
        {
            ["timestamp"] = DateTime.UtcNow.ToString("o"),
            ["level"] = level.ToString().ToUpperInvariant(),
            ["component"] = _component,
            ["message"] = message
        };

        if (!string.IsNullOrEmpty(requestId))
        {
            entry["requestId"] = requestId;
        }

        if (data != null)
        {
            entry["data"] = data;
        }

        if (exception != null)
        {
            entry["error"] = new
            {
                message = exception.Message,
                stackTrace = exception.StackTrace,
                type = exception.GetType().Name
            };
        }

        return entry;
    }
}
