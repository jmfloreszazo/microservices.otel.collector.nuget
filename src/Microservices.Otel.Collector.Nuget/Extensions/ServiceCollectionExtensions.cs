using Microservices.Otel.Collector.Nuget.AppConfigurations;
using Microservices.Otel.Collector.Nuget.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Microservices.Otel.Collector.Nuget.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenTelemetryLogger(this IServiceCollection services, IConfigurationRoot config)
    {
        var loggerFactory = LoggerFactory.Create(loggingBuilder =>
        {
            loggingBuilder.AddConfiguration(config);
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
            loggingBuilder.AddOpenTelemetry(options =>
            {
                options.IncludeScopes = true;
                options.ParseStateValues = true;
                options.IncludeFormattedMessage = true;
                options.AddConsoleExporter();
            });
        });
        var applicationName = config.GetSection(AppSettingsConstants.OpenTelemetryConfiguration)
            .Get<AppConfiguration>()
            ?.OpenTelemetry.Source;
        if (applicationName == null) return services;
        var logger = loggerFactory.CreateLogger(applicationName);
        services.AddSingleton(logger);
        return services;
    }


    public static IServiceCollection AddOpenTelemetryTracingMetrics(this IServiceCollection services, IConfigurationRoot config)
    {
        var openTelemetryEndPoint = config.GetSection(AppSettingsConstants.OpenTelemetryConfiguration)
            .Get<AppConfiguration>()
            ?.OpenTelemetry?.EndPoint;
        var openTelemetrySource = config.GetSection(AppSettingsConstants.OpenTelemetryConfiguration)
            .Get<AppConfiguration>()
            ?.OpenTelemetry?.Source;
        services.AddOpenTelemetry()
            .WithTracing(builder => builder
                .AddSource(openTelemetrySource)
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName: openTelemetrySource ?? OtelConstans.DefaultActivitySourceName))
                .AddHttpClientInstrumentation((options) =>
                {
                    options.EnrichWithException = (activity, exception) =>
                    {
                        activity.SetTag(OtelConstans.ExceptionType, exception.StackTrace);
                    };
                })
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpResponse = (activity, httpResponse) =>
                    {
                        activity.SetTag("CorrelationID", httpResponse.Headers["CorrelationID"]);
                    };
                    options.EnrichWithException = (activity, exception) =>
                    {
                        activity.SetTag(OtelConstans.StackTrace, exception.GetType().ToString());
                        activity.SetTag(OtelConstans.ExceptionType, exception.StackTrace);
                    };
                })
                .AddConsoleExporter()
                .AddOtlpExporter(exporterOptions =>
                {
                    if (openTelemetryEndPoint != null) exporterOptions.Endpoint = new Uri(openTelemetryEndPoint);
                }))
            .WithMetrics(builder => builder
                .AddAspNetCoreInstrumentation()
                .AddConsoleExporter()
                .AddOtlpExporter((exporterOptions, readerOptions) =>
                {
                    if (openTelemetryEndPoint != null) exporterOptions.Endpoint = new Uri(openTelemetryEndPoint);
                    readerOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 5000;
                })
                .AddView("*",
                    new ExplicitBucketHistogramConfiguration
                    {
                        Boundaries = new double[] { 0, 500, 1000, 2500, 5000, 7500, 10000, 25000, 50000 },
                        RecordMinMax = true
                    })
                .ConfigureResource(resource =>
                {
                    var applicationName = config.GetSection(AppSettingsConstants.OpenTelemetryConfiguration)
                        .Get<AppConfiguration>()
                        ?.OpenTelemetry.Source;
                    if (applicationName != null) resource.AddService(applicationName);
                }));

        return services;
    }
}