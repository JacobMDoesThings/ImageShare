namespace ImageShare.Shared.Services.BlobService;

public interface IFileUploadService
{
    public Task<bool> UploadImageAsync(Stream fileStream, bool isPublic);
}