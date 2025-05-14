namespace ImageShare.ImageBlobService;

public class BlobServiceConfiguration
{
    public required string? BaseAddress { get; init; }
    public required string? SasEndPoint { get; init; }
}