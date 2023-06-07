using System.Net;

namespace Microservices.Otel.Collector.Nuget.Errors;

public class BaseException : Exception
{
    public string? Messages { get; }

    public HttpStatusCode StatusCode { get; }

    public BaseException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        : base(message)
    {
        Messages = message;
        StatusCode = statusCode;
    }
}