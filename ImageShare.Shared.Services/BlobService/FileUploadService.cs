using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ImageShare.Shared.Services.BlobService.Configuration;

namespace ImageShare.Shared.Services.BlobService;

public class FileUploadService : IFileUploadService
{
    private readonly HttpClient _sasServiceClient;
    private readonly BlobServiceConfiguration _blobServiceConfiguration;

    public FileUploadService(BlobServiceConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _sasServiceClient = httpClientFactory.CreateClient("SasService");
        if (configuration.BaseAddress is not null) _sasServiceClient.BaseAddress = new Uri(configuration.BaseAddress);
        _blobServiceConfiguration = configuration;
    }

    public async Task<bool> UploadImageAsync(Stream fileStream, bool isPublic = false)
    {
        var groupId = "groupid1";
        var urlWithParams = $"{_blobServiceConfiguration.SasEndPoint}?selectedGroupId={groupId}&isPublic={isPublic}";
        var response = await _sasServiceClient.GetAsync(urlWithParams);
        if (!response.IsSuccessStatusCode) return false;
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        string sasUrl;
        try
        {
            JsonNode node = JsonNode.Parse(result?.ToString()); 
            sasUrl = node["sasUrl"]?.GetValue<string>() ?? string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        
        try
        {
            var blobHttpHeader = new BlobHttpHeaders { ContentType = "image/jpeg" };
            var blobClient = new BlobClient(new Uri(sasUrl));
            await blobClient.UploadAsync(fileStream, overwrite: false);
            await blobClient.SetHttpHeadersAsync(blobHttpHeader);
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
}