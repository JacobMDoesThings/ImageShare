using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using ImageShare.WebApi.Configuration;
using ImageShare.WebApi.Data;

namespace ImageShare.WebApi.Services;

public class SasGeneratorService(AzureStorageConfiguration azStConfiguration) : ISasGeneratorService

{
    public async Task<string> GenerateWriteSasTokenAsync(UserImageInfo userImageInfo)
    {
        try
        {
            var containerClient = await GetBlobContainerClientAsync(GetBlobServiceClient(), userImageInfo);
            return GenerateWriteSasUrl(containerClient, userImageInfo);
        }
        catch (Exception ex)
        {
            throw new SasGeneratorException("Failed to generate write sas url", ex);
        }
    }

    public async Task<IEnumerable<string>> BulkGenerateWriteSasUrlAsync(
        UserImageInfo userImageInfo, int count)
    {
        List<string> sasUrls = [];
        try
        {
            var blobServiceClient = await GetBlobContainerClientAsync(GetBlobServiceClient(), userImageInfo);
            for (var i = 0; i < count; i++)
            {
                sasUrls.Add(GenerateWriteSasUrl(blobServiceClient, userImageInfo));
            }
            return sasUrls;
        }
        catch (Exception ex)
        {
            throw new SasGeneratorException("Failed to generate write sas url during bulk transaction", ex);
        }
    }

    private string GenerateWriteSasUrl(BlobContainerClient blobContainerClient, 
        UserImageInfo userImageInfo)
    {
        var blobName = GenerateBlobName(userImageInfo);
        var blobClient = blobContainerClient.GetBlobClient(blobName);
        var sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = userImageInfo.SelectedGroupId,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10),
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Write);

        var sasToken = sasBuilder.ToSasQueryParameters(
            new StorageSharedKeyCredential(azStConfiguration.AccountName, azStConfiguration.AccountKey)).ToString();
        
        return $"{blobClient.Uri}?{sasToken}";
    }

    private BlobServiceClient GetBlobServiceClient()
    {
        var blobServiceClient = new BlobServiceClient(
            new Uri($"https://{azStConfiguration.AccountName}.blob.core.windows.net"),
            new StorageSharedKeyCredential(azStConfiguration.AccountName, azStConfiguration.AccountKey)
        );
        
        return blobServiceClient;
    }

    private static async Task<BlobContainerClient> GetBlobContainerClientAsync(BlobServiceClient blobServiceClient,
        UserImageInfo userImageInfo)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(userImageInfo.SelectedGroupId);

        var createResponse = await containerClient.CreateIfNotExistsAsync();

        if (createResponse?.GetRawResponse().Status != 201 && createResponse is not null)
        {
            throw new SasGeneratorException($"Exception while creating blob container: " +
                                            $"{createResponse.GetRawResponse().Content}");
        }
        if (createResponse is not null)
        {
            await CheckContainerAvailabilityAsync(containerClient);
        }
        
        return containerClient;
    }

    private static async Task CheckContainerAvailabilityAsync(BlobContainerClient blobContainerClient)
    {
        var containerAvailable = false;
        var retryCount = 0;
        var delay = TimeSpan.FromSeconds(1);

        while (!containerAvailable && retryCount < 5)
        {
            try
            {
                await blobContainerClient.GetPropertiesAsync();
                containerAvailable = true;
            }
            catch (RequestFailedException ex) when (
                ex.Status is 404)
            {
                retryCount++;
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds);
            }
        }

        if (!containerAvailable)
        {
            throw new SasGeneratorException("Container not available after retries.");
        }
    }

    private static string GenerateBlobName(UserImageInfo userImageInfo)
    {
        return  userImageInfo.IsPublicImage
            ? $"public/images/{userImageInfo.UserId}/{Guid.NewGuid()}-{DateTime.Now.Ticks}"
            : $"{userImageInfo.UserId}/images/{Guid.NewGuid()}-{DateTime.Now.Ticks}";
    }

    private class SasGeneratorException : Exception
    {
        public SasGeneratorException(string message):base(message){}
        public SasGeneratorException(string message, Exception innerException):base(message, innerException){}
    }
}