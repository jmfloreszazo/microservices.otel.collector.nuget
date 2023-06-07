using System.Net;

namespace Microservices.Otel.Collector.Nuget.Errors;

public class BadRequestException : BaseException
{
    public BadRequestException(string message)
        : base(message, HttpStatusCode.NotFound)
    {
    }
}