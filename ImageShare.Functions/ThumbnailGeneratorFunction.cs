using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure.Messaging.EventGrid;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using ImageShare.Functions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using ImageShare.Functions.Data;
using ImageShare.Functions.Interfaces;

namespace ImageShare.Functions;

public class ThumbnailGenerationFunction(
    ThumbnailGenerationConfiguration thumbnailConfig,
    IBlobUrlParser blobUrlParser,
    IDataLakeServiceClientFactory dataLakeFactory,
    IImageValidator imageValidator,
    ILogger<ThumbnailGenerationFunction> logger)
{
    [Function("EventGridThumbnailGenerator")]
    public async Task Run(
        [EventGridTrigger] EventGridEvent events,
        FunctionContext context)
    {
        logger.LogInformation("Processing Event Grid event");

        try
        {
            var blobUrl = JsonDocument.Parse(events.Data).RootElement.GetProperty("url").GetString();

            if (blobUrl is not null)
            {
                var blobData = blobUrlParser.ParseBlob(new Uri(blobUrl));

                logger.LogInformation("FileSystem: {ContainerName}, User: {User}, Name: {Name}",
                    blobData.ContainerName, blobData.User, blobData.Name);

                await ProcessImageAsync(blobData);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Event Grid event");
        }
    }

    internal async Task ProcessImageAsync(BlobData blobData)
    {
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

    internal async Task ProcessImagePipelineAsync(
        BlobData blobData,
        DataLakeFileSystemClient fileSystemClient)
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

    internal DataLakeFileClient GetSourceFileClient(
        DataLakeFileSystemClient fileSystemClient,
        BlobData blobData)
    {
        var fullPath = blobData.BlobUrlString;
        logger.LogInformation("Source File Path: {FilePath}", fullPath);

        return fileSystemClient.GetFileClient(fullPath);
    }

    internal async Task ValidateMimeTypeAsync(DataLakeFileClient fileClient)
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

    internal async Task<MemoryStream> DownloadFileStreamAsync(DataLakeFileClient fileClient)
    {
        var downloadResponse = await fileClient.ReadAsync();

        var inputBlobStream = new MemoryStream();
        await downloadResponse.Value.Content.CopyToAsync(inputBlobStream);

        inputBlobStream.Position = 0;
        return inputBlobStream;
    }

    internal async Task ValidateImageStructureAsync(MemoryStream stream)
    {
        try
        {
            await imageValidator.Validate(stream);
        }
        catch (Exception ex)
        {
            logger.LogWarning("Invalid image structure detected: {Error}", ex.Message);
            throw;
        }
    }

    internal async Task<MemoryStream> GenerateThumbnailAsync(MemoryStream inputBlobStream)
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

    internal DataLakeFileClient GetThumbnailFileClient(
        DataLakeFileSystemClient fileSystemClient,
        BlobData blobData)
    {
        var thumbnailName = $"{blobData.Name}-thumb";

        var thumbPath = blobData.IsPublic
            ? $"{thumbnailConfig.ThumbnailPathPublic}/{blobData.User}/{thumbnailName}"
            : $"{thumbnailConfig.ThumbnailPathPrivate.ReplacePlaceholder(blobData.User)}/{thumbnailName}";

        return fileSystemClient.GetFileClient(thumbPath);
    }

    internal async Task UploadThumbnailAsync(
        DataLakeFileClient thumbFileClient,
        MemoryStream thumbnailBytes)
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

    internal async Task SetThumbnailMetadataAsync(DataLakeFileClient thumbFileClient)
    {
        var tags = new Dictionary<string, string>
        {
            { "CreatedOn", DateTime.UtcNow.ToString("o") }
        };

        await thumbFileClient.SetMetadataAsync(tags);
    }
}