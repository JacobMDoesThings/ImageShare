using Azure.Storage.Files.DataLake;
using ImageShare.Functions.Interfaces;

namespace ImageShare.Functions.Services;

public class DataLakeServiceClientFactory : IDataLakeServiceClientFactory
{
    public DataLakeServiceClient Create(string connectionString)
    {
        return new DataLakeServiceClient(connectionString);
    }
}
