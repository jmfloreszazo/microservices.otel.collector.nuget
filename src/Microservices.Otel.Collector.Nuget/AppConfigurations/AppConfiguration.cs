namespace Microservices.Otel.Collector.Nuget.AppConfigurations;

public class AppConfiguration
{
    public OpenTelemetry OpenTelemetry { get; set; }
}

public class OpenTelemetry
{
    public string Source { get; set; }
    public string EndPoint { get; set; }
}