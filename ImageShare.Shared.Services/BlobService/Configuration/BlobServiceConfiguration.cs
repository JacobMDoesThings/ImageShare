namespace ImageShare.Shared.Services.BlobService.Configuration;

public class BlobServiceConfiguration
{
    public required string? BaseAddress { get; init; }
    public required string? SasEndPoint { get; init; }
}