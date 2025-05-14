using ImageShare.Functions.Interfaces;
using SixLabors.ImageSharp;

namespace ImageShare.Functions.Services;

public class ImageValidator : IImageValidator
{
    public async Task Validate(MemoryStream stream)
    {
        try
        {
            await Image.LoadAsync(stream); // Throws exceptions for invalid images
        }
        catch (Exception ex)
        {
            throw new ImageValidationException("Invalid image structure detected", ex);
        }
    }
}

public class ImageValidationException(string message, Exception innerException) : Exception(message, innerException);
