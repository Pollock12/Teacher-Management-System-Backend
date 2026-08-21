using System.Text.Json;
using FluentValidation;
using TMS.API.Models;
using TMS.Domain.Exceptions;

namespace TMS.API.Middleware;

/// <summary>
/// Catches all unhandled exceptions in the ASP.NET Core pipeline and converts them
/// to a structured <see cref="ErrorResponse"/> JSON payload with an appropriate HTTP
/// status code. A <c>correlationId</c> is generated per request for log tracing.
/// </summary>


/*
   A middleware is something that sits in the ASP.NET Core HTTP request pipeline.
   Request -> GlobalExceptionMiddleware -> Authentication -> Controller -> Application Handler -> Repository -> MongoDB

*/
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    // Shared serializer options — camelCase to match the JSON contract in the design doc.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid();

        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await WriteErrorAsync(context, correlationId,
                statusCode: StatusCodes.Status400BadRequest,
                error: "Bad Request",
                message: "One or more validation errors occurred.",
                details: ex.Errors
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                    .ToList());
        }
        catch (NotFoundException ex)
        {
            await WriteErrorAsync(context, correlationId,
                statusCode: StatusCodes.Status404NotFound,
                error: "Not Found",
                message: ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteErrorAsync(context, correlationId,
                statusCode: StatusCodes.Status409Conflict,
                error: "Conflict",
                message: ex.Message);
        }
        catch (DomainRuleException ex)
        {
            await WriteErrorAsync(context, correlationId,
                statusCode: StatusCodes.Status422UnprocessableEntity,
                error: "Unprocessable Entity",
                message: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);

            await WriteErrorAsync(context, correlationId,
                statusCode: StatusCodes.Status500InternalServerError,
                error: "Internal Server Error",
                message: "An unexpected error occurred. Please try again later.");
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static Task WriteErrorAsync(
        HttpContext context,
        Guid correlationId,
        int statusCode,
        string error,
        string message,
        IReadOnlyList<string>? details = null)
    {
        var response = new ErrorResponse
        {
            CorrelationId = correlationId,
            Status        = statusCode,
            Error         = error,
            Message       = message,
            Details       = details ?? []
        };

        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }
}
