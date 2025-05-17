using ImageShare.Shared.Services.BlobService;
using ImageShare.Shared.Services.BlobService.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.FluentUI.AspNetCore.Components;

namespace ImageShare.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddFluentUIComponents();
        //builder.Services.AddHttpClient();

// #if DEBUG
//         // Bypass certificate validation during development builds ONLY!
//         builder.Services.AddHttpClient("SasService", client =>
//             {
//                 client.BaseAddress = new Uri("https://localhost:7263"); // Your backend API URL
//             })
//             .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
//             {
//                 ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
//             });
//         builder.Services.AddBlazorWebViewDeveloperTools();
//         builder.Logging.AddDebug();
// #else
//         // Use secure certificate validation in production builds
//         builder.Services.AddHttpClient("SasService", client =>
//         {
//             client.BaseAddress = new Uri("https://your-production-api.com"); // Replace with production URL
//         });
// #endif
        // Load appsettings.json manually
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "appsettings.json");
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("wwwroot/appsettings.json", optional: false, reloadOnChange: true);

// Optional: Add environment-specific files
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        configBuilder.AddJsonFile($"wwwroot/appsettings.{env}.json", optional: true);

        var config = configBuilder.Build();

// Register IConfiguration as a service
        builder.Configuration.AddConfiguration(config);
        builder.Services.AddSingleton(config);
        builder.Services.AddSingleton(new BlobServiceConfiguration()
        {
            BaseAddress = config["SasService:BaseAddress"],
            SasEndPoint = config["SasService:SasEndPoint"]
        });

        builder.Services.AddHttpClient("SasService",
                client => { client.BaseAddress = new Uri(config["sasService:BaseAddress"] ?? string.Empty); })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
#if DEBUG
                // Bypass certificate validation during development builds ONLY!
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif
                return handler;
            });

        builder.Services.AddSingleton<IFileUploadService, FileUploadService>();
        return builder.Build();
    }
}