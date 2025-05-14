namespace ImageShare.Functions.Interfaces;

public interface IImageValidator
{
    Task Validate(MemoryStream stream);
}