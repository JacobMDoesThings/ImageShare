using ImageShare.Functions.Data;
using ImageShare.Functions.Interfaces;

namespace ImageShare.Functions.Services;

public class BlobUrlParser : IBlobUrlParser
{
    public BlobData ParseBlob(Uri blobUrl)
    {
        var pathSegments = blobUrl.Segments.Skip(1) // Skip "https://<account>.dfs.core.windows.net/"
            .Select(s => s.Trim('/'))
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();

        var isPublic = pathSegments[1].Equals("public");

        return new BlobData()
        {
            BlobUrlString = $"{string.Join("/", pathSegments).Replace(pathSegments[0],"" )}", // Relative minus container
            ContainerName = pathSegments[0], // First segment is filesystem name
            User = isPublic ? pathSegments[3] : pathSegments[1],
            Name = isPublic ? pathSegments[4] : pathSegments[3],
            IsPublic = pathSegments[1].Equals("public")
        };
    }
}