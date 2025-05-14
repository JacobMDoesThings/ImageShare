using Azure.Storage;
using ImageShare.WebApi.Configuration;
using ImageShare.WebApi.Services;

namespace ImageShare.WebApi;

using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

[ApiController]
[Route("api/[controller]")]
public class SasController(AzureStorageConfiguration azStConfiguration, IUserContextProvider userContextProvider)
    : ControllerBase
{
    [HttpGet("generate")]
    [RequestFormLimits(MultipartBodyLengthLimit = (10 * 1024 * 1024))]
    public IActionResult GenerateSasTokenForUpload(bool isPublic = false)
     {
         var userId = userContextProvider.GetUserId();
         try
         {
             // Create BlobServiceClient with shared key credential
             var blobServiceClient = new BlobServiceClient(
                 new Uri($"https://{azStConfiguration.AccountName}.blob.core.windows.net"),
                 new StorageSharedKeyCredential(azStConfiguration.AccountName, azStConfiguration.AccountKey)
             );

             // Create container client with user ID as container name
             var containerClient = blobServiceClient.GetBlobContainerClient(azStConfiguration.MainContainerName);

             //Create container if it doesn't exist with PrivateBlob access policy
             var createResponse = containerClient.CreateIfNotExists();
             
             if (createResponse?.GetRawResponse().Status != 201 && createResponse is not null)
             {
                 return Problem("Failed to create container.");
             }

             var blobName =
                 // Generate unique blob name
                 isPublic ? $"public/images/{userId}/{Guid.NewGuid()}-{DateTime.Now.Ticks}" : 
                 $"{userId}/images/{Guid.NewGuid()}-{DateTime.Now.Ticks}";

             // Get blob client using SDK method instead of manual path building
             var blobClient = containerClient.GetBlobClient(blobName);

             // Generate SAS token with write permissions valid for 1 hour
             BlobSasBuilder sasBuilder = new BlobSasBuilder()
             {
                 BlobContainerName = azStConfiguration.MainContainerName,
                 BlobName = blobName,
                 Resource = "b",
                 ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10),
             };
             
             sasBuilder.SetPermissions(BlobSasPermissions.Write);

             string sasToken = sasBuilder.ToSasQueryParameters(
                 new StorageSharedKeyCredential(azStConfiguration.AccountName, azStConfiguration.AccountKey)).ToString();

             // Construct SAS URL using blobClient.Uri instead of manual path building
             string sasUrl = $"{blobClient.Uri}?{sasToken}";

             return Ok(new { SasUrl = sasUrl });
         }
         catch (Exception ex)
         {
             return Problem($"Error generating SAS token: {ex.Message}");
         }
     }
}