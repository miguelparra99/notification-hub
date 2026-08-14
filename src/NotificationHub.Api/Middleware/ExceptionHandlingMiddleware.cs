using System.Net;
using System.Text.Json;
using FluentValidation;
using NotificationHub.Domain.Exceptions;

namespace NotificationHub.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (status, title, errors) = exception switch
        {
            ValidationException v => (
                HttpStatusCode.BadRequest,
                "Validation failed.",
                v.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToArray()),

            DomainException d => (
                HttpStatusCode.UnprocessableEntity,
                d.Message,
                Array.Empty<string>()),

            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred.",
                Array.Empty<string>())
        };

        if (status == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception on {Path}.", context.Request.Path);

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/problem+json";

        var payload = JsonSerializer.Serialize(new
        {
            type = $"https://httpstatuses.io/{(int)status}",
            title,
            status = (int)status,
            errors,
            traceId = context.TraceIdentifier
        });

        await context.Response.WriteAsync(payload);
    }
}