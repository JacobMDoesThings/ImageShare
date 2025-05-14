namespace ImageShare.WebApi.Configuration;

public class AzureStorageConfiguration
{
    public required string AccountName { get; init; }
    public required string AccountKey { get; init; }
    public required string MainContainerName { get; init; }
    
    internal bool IsValid()
        => !string.IsNullOrEmpty(AccountName) && 
           !string.IsNullOrEmpty(AccountKey) && 
           !string.IsNullOrEmpty(MainContainerName);
}