namespace ImageShare.WebApi.Configuration;

public static class ConfigurationExtensions
{
    public static void AddAzureStorageConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var configSection = configuration.GetRequiredSection("AzureStorage");

        // Bind section to AzureStorageConfiguration
        services.Configure<AzureStorageConfiguration>(configSection);

        // Retrieve configuration instance for validation
        var azureConfig = configSection.Get<AzureStorageConfiguration>();

        if (string.IsNullOrEmpty(azureConfig?.AccountName))
            throw new InvalidOperationException("Azure Storage AccountName is required.");

        if (string.IsNullOrEmpty(azureConfig?.AccountKey))
            throw new InvalidOperationException("Azure Storage AccountKey is required.");
        
        services.AddSingleton(azureConfig);
    }
}
