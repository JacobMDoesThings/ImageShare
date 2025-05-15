using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using ImageShare.Functions;
using ImageShare.Functions.Configuration;
using ImageShare.Functions.Interfaces;
using ImageShare.Functions.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("ImageShare.Functions.Tests.Unit")]
var builder = FunctionsApplication.CreateBuilder(args);

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();
var config = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

// Bind configuration section to ThumbnailConfig class
var thumbnailConfig = config.GetSection("ThumbnailGenerationConfiguration").Get<ThumbnailGenerationConfiguration>();
if (thumbnailConfig == null)
    throw new Exception("Missing thumbnail configuration");
// Validate configuration
var validationResults = new List<ValidationResult>();
if (!Validator.TryValidateObject(thumbnailConfig, new ValidationContext(thumbnailConfig),
        validationResults, true))
{
    var errors = string.Join("\n", validationResults.Select(r => r.ErrorMessage));
    throw new InvalidOperationException($"Configuration validation failed:\n{errors}");
}
builder.Services.AddSingleton<IDataLakeServiceClientFactory, DataLakeServiceClientFactory>();
builder.Services.AddSingleton<IImageValidator, ImageValidator>();
builder.Services.AddSingleton<IBlobUrlParser, BlobUrlParser>();
// Register logger factory
//var serviceProvider = builder.Services.BuildServiceProvider();
var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole());
        
// Register ILogger<T> via factory
builder.Services.AddSingleton(loggerFactory.CreateLogger<ThumbnailGenerationFunction>());

builder.Services.AddSingleton<ThumbnailGenerationConfiguration>(sp => new ThumbnailGenerationConfiguration(){
    ConnectionString = thumbnailConfig.ConnectionString,
    JpegQuality = thumbnailConfig.JpegQuality,
    AllowedMimeTypes = thumbnailConfig.AllowedMimeTypes,
    MaxHeight = thumbnailConfig.MaxHeight,
    ThumbnailPathPrivate = thumbnailConfig.ThumbnailPathPrivate,
    ThumbnailPathPublic = thumbnailConfig.ThumbnailPathPublic
});

builder.Services.AddSingleton<IImageProcessingService, ImageProcessingService>();


// Configure web app (required for isolated worker)
builder.ConfigureFunctionsWebApplication();

builder.Build().Run();