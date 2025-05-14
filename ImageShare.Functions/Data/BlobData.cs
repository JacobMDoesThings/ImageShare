namespace ImageShare.Functions.Data;

public class BlobData
{
   public required string BlobUrlString { get; init; }
   public required string ContainerName { get; init; }
   public required string User { get; init; }
   public required string Name { get; init; }
   public required bool IsPublic { get; init; }
   
   public BlobData(string blobUrlString, string containerName, string user, string name, bool isPublic)
   {
      BlobUrlString = blobUrlString;
      ContainerName = containerName;
      User = user;
      Name = name;
      IsPublic = isPublic;
   }

   public BlobData()
   {
   }
}