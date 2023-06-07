using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microservices.Otel.Collector.Nuget.Constants;
using Microservices.Otel.Collector.Nuget.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Microservices.Otel.Collector.Nuget.Middlewares;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public ErrorHandlerMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BadRequestException ex)
        {
            ActivitySource activitySource =
                new(_configuration.GetValue<string>(AppSettingsConstants.OpenTelemetryConfigurationSource) ??
                    OtelConstans.DefaultActivitySourceName);
            using var activity = activitySource.StartActivity(HttpStatusCode.NotFound.ToString());
            activity?.SetTag(OtelConstans.ExceptionType, HttpStatusCode.NotFound.ToString());
            activity?.SetTag(OtelConstans.StackTrace, ex.StackTrace);
            ComposeResponseError(context, HttpStatusCode.NotFound, ex.Message, ex.StackTrace);
        }
    }

    private static async void ComposeResponseError(HttpContext context, HttpStatusCode httpStatusCode, string message,
        string? stackTrace)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        context.Response.StatusCode = (int)httpStatusCode;
        var response = new
        {
            HttpStatusCode = httpStatusCode,
            Message = message,
            Exception = stackTrace
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}