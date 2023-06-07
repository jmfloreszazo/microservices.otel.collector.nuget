using Microservices.Otel.Collector.Nuget.AppConfigurations;
using Microservices.Otel.Collector.Nuget.Constants;
using Microservices.Otel.Collector.Nuget.Extensions;
using Microservices.Otel.Collector.Nuget.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppConfiguration>(builder.Configuration.GetSection(AppSettingsConstants.OpenTelemetryConfiguration));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetryLogger(builder.Configuration);
builder.Services.AddOpenTelemetryTracingMetrics(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();