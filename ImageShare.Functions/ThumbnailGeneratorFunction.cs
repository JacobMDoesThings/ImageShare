using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure.Messaging.EventGrid;
using ImageShare.Functions.Interfaces;

namespace ImageShare.Functions;

public class ThumbnailGenerationFunction(
    IImageProcessingService imageProcessingService,
    ILogger<ThumbnailGenerationFunction> logger)
{
    [Function("EventGridThumbnailGenerator")]
    public async Task Run(
        [EventGridTrigger] EventGridEvent gridEvent,
        FunctionContext context)
    {
        logger.LogInformation("Processing Event Grid event");

        logger.LogInformation("Received event type: {EventType}", gridEvent.EventType);


        // Handle blob events
        if (gridEvent.EventType.Equals("Microsoft.Storage.BlobCreated"))
        {
            try
            {
                var blobData = JsonDocument.Parse(gridEvent.Data.ToString());
                var blobUrl = blobData.RootElement.GetProperty("url").GetString();

                logger.LogInformation("Processing blob URL: {BlobUrl}", blobUrl);

                if (blobUrl is not null)
                {
                    await imageProcessingService.ProcessImageAsync(new Uri(blobUrl));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing blob event");
            }
        }
    }
}