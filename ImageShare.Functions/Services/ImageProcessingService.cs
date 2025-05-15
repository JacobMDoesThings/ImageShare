using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using ImageShare.Functions.Configuration;
using ImageShare.Functions.Data;
using ImageShare.Functions.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace ImageShare.Functions.Services;

public class ImageProcessingService(
    ILogger<ImageProcessingService> logger,
    ThumbnailGenerationConfiguration thumbnailConfig,
    IBlobUrlParser blobUrlParser,
    IDataLakeServiceClientFactory dataLakeFactory,
    IImageValidator imageValidator): IImageProcessingService
{
    
    public async Task ProcessImageAsync(Uri blobUri)
    {
        var blobData =  blobUrlParser.ParseBlob(blobUri);
        
        logger.LogInformation("Processing image {Name} for user {User}",
            blobData.Name, blobData.User);
        
        try
        {
            var serviceClient = dataLakeFactory.Create(thumbnailConfig.ConnectionString);
            var fileSystemClient = serviceClient.GetFileSystemClient(blobData.ContainerName);

            await ProcessImagePipelineAsync(
                blobData,
                fileSystemClient
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing image pipeline");
            throw;
        }
    }

    public async Task ProcessImagePipelineAsync(BlobData blobData, DataLakeFileSystemClient fileSystemClient)
    {
        var sourceFileClient = GetSourceFileClient(fileSystemClient, blobData);
        
        logger.LogInformation("Validating MIME type...");
        await ValidateMimeTypeAsync(sourceFileClient);
        
        logger.LogInformation("Downloading file...");
        var inputBlobStream = await DownloadFileStreamAsync(sourceFileClient);

        logger.LogInformation("Validating image structure...");
        await ValidateImageStructureAsync(inputBlobStream);

        logger.LogInformation("Generating thumbnail...");
        var thumbnailBytes = await GenerateThumbnailAsync(inputBlobStream);

        logger.LogInformation("Uploading thumbnail...");
        var thumbFileClient = GetThumbnailFileClient(fileSystemClient, blobData);
        await UploadThumbnailAsync(thumbFileClient, thumbnailBytes);

        logger.LogInformation("Processing completed successfully");
    }

    public virtual DataLakeFileClient GetSourceFileClient(DataLakeFileSystemClient fileSystemClient, BlobData blobData)
    {
        var fullPath = blobData.BlobUrlString;
        logger.LogInformation("Source File Path: {FilePath}", fullPath);

        return fileSystemClient.GetFileClient(fullPath);
    }

    public virtual async Task ValidateMimeTypeAsync(DataLakeFileClient fileClient)
    {
        var properties = await fileClient.GetPropertiesAsync();
        var mimeType = properties.Value.ContentType;

        if (!thumbnailConfig.AllowedMimeTypes.Contains(mimeType))
        {
            logger.LogWarning("Invalid MIME type {Mime} detected", mimeType);
            await fileClient.DeleteAsync();
            throw new InvalidOperationException($"Invalid MIME type {mimeType}");
        }
    }

    public virtual async Task<MemoryStream> DownloadFileStreamAsync(DataLakeFileClient fileClient)
    {
        var downloadResponse = await fileClient.ReadAsync();

        var inputBlobStream = new MemoryStream();
        await downloadResponse.Value.Content.CopyToAsync(inputBlobStream);

        inputBlobStream.Position = 0;
        return inputBlobStream;
    }

    public virtual async Task ValidateImageStructureAsync(MemoryStream stream)
    {
        try
        {
            await imageValidator.Validate(stream);
        }
        catch (Exception ex)
        {
            logger.LogWarning("Invalid image structure detected: {Error}", ex.Message);
        }
    }

    public virtual async Task<MemoryStream> GenerateThumbnailAsync(MemoryStream inputBlobStream)
    {
        inputBlobStream.Position = 0;
        using var image = await Image.LoadAsync(inputBlobStream);

        if (image.Height <= thumbnailConfig.MaxHeight)
        {
            await image.SaveAsJpegAsync(inputBlobStream,
                new JpegEncoder());
            inputBlobStream.Position = 0;
            return inputBlobStream;
        }

        var aspectRatio = image.Width / (float)image.Height;
        var targetWidth = Math.Min((int)(thumbnailConfig.MaxHeight * aspectRatio), image.Width);

        image.Mutate(x => x.Resize(targetWidth, thumbnailConfig.MaxHeight));

        var outputStream = new MemoryStream();
        await image.SaveAsJpegAsync(outputStream,
            new JpegEncoder { Quality = thumbnailConfig.JpegQuality });

        outputStream.Position = 0;

        return outputStream;
    }

    public virtual DataLakeFileClient GetThumbnailFileClient(DataLakeFileSystemClient fileSystemClient, BlobData blobData)
    {
        var thumbnailName = $"{blobData.Name}-thumb";

        var thumbPath = blobData.IsPublic
            ? $"{thumbnailConfig.ThumbnailPathPublic}/{blobData.User}/{thumbnailName}"
            : $"{thumbnailConfig.ThumbnailPathPrivate.ReplacePlaceholder(blobData.User)}/{thumbnailName}";

        return fileSystemClient.GetFileClient(thumbPath);
    }

    public virtual async Task UploadThumbnailAsync(DataLakeFileClient thumbFileClient, MemoryStream thumbnailBytes)
    {
        await thumbFileClient.UploadAsync(thumbnailBytes, overwrite: true);

        if (thumbFileClient.Path.StartsWith("public/", StringComparison.CurrentCultureIgnoreCase))
        {
            await SetThumbnailMetadataAsync(thumbFileClient);
            logger.LogInformation("Thumbnail metadata updated for public, e.g. CreatedOn");
        }

        await thumbFileClient.SetHttpHeadersAsync(new PathHttpHeaders() { ContentType = "image/jpeg" });

        logger.LogInformation($"Thumbnail uploaded successfully with MIME type: image/jpeg");
    }

    public virtual async Task SetThumbnailMetadataAsync(DataLakeFileClient thumbFileClient)
    {
        var tags = new Dictionary<string, string>
        {
            { "CreatedOn", DateTime.UtcNow.ToString("o") }
        };

        await thumbFileClient.SetMetadataAsync(tags);
    }
}