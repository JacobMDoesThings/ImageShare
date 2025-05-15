using ImageShare.Functions.Data;

namespace ImageShare.Functions.Interfaces;

public interface IBlobUrlParser
{
    public BlobData ParseBlob(Uri blobUrl);
}