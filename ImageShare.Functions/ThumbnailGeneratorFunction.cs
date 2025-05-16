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
        [EventGridTrigger] EventGridEvent[] events,
        FunctionContext context)
    {
        logger.LogInformation("Processing Event Grid event");

        foreach (var eventGridEvent in events)
        {
            logger.LogInformation("Received event type: {EventType}", eventGridEvent.EventType);

            switch (eventGridEvent.EventType)
            {
                // Handle subscription validation events
                case "Microsoft.EventGrid.SubscriptionValidationEvent":
                {
                    logger.LogInformation("Handling subscription validation event");

                    // Parse validation data
                    var validationEvent = JsonDocument.Parse(eventGridEvent.Data.ToString());
                    var validationCode = validationEvent.RootElement.GetProperty("validationCode").GetString();

                    logger.LogInformation("Validation code received: {ValidationCode}", validationCode);

                    // Respond with HTTP 200 OK and validationResponse matching Azure's challenge
                    var response = new 
                    {
                        validationResponse = validationCode
                    };

                    logger.LogInformation("Responding to subscription validation");
                    return;
                }
                // Handle blob events
                case "Microsoft.Storage.BlobCreated":
                    try
                    {
                        var blobData = JsonDocument.Parse(eventGridEvent.Data.ToString());
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

                    break;
            }
        }

        logger.LogInformation("No events processed.");
    }
}