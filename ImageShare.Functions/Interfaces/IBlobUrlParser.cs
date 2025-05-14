using ImageShare.Functions.Data;

namespace ImageShare.Functions.Interfaces;

public interface IBlobUrlParser
{
    BlobData ParseBlob(Uri blobUrl);
}