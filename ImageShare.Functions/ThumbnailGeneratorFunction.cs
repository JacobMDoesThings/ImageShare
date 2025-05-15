using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure.Messaging.EventGrid;
using ImageShare.Functions.Interfaces;

namespace ImageShare.Functions;

public class ThumbnailGenerationFunction(
    IImageProcessingService imageProcessingService, ILogger<ThumbnailGenerationFunction> logger)
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
                await imageProcessingService.ProcessImageAsync(new Uri(blobUrl));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Event Grid event");
        }
    }
}