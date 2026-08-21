namespace TMS.API.Models;

/// <summary>
/// Structured error response returned by <see cref="TMS.API.Middleware.GlobalExceptionMiddleware"/>
/// for all handled and unhandled exceptions.
/// </summary>
public sealed class ErrorResponse
{
    /// <summary>Per-request correlation identifier for log tracing.</summary>
    // This is especially useful for debugging production issues.
    public Guid CorrelationId { get; init; }

    /// <summary>HTTP status code (e.g. 400, 404, 409, 422, 500).</summary>
    public int Status { get; init; }

    /// <summary>Short error category (e.g. "Bad Request", "Not Found").</summary>
    public string Error { get; init; } = string.Empty;

    /// <summary>Human-readable explanation of what went wrong.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Optional list of field-level validation errors in "FieldName: ErrorMessage" format.
    /// Empty for non-validation errors.
    /// </summary>
    public IReadOnlyList<string> Details { get; init; } = [];
}
