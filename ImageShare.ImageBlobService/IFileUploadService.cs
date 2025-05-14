namespace ImageShare.ImageBlobService;

public interface IFileUploadService
{
    public Task<bool> UploadImageAsync(Stream fileStream, bool isPublic);
}