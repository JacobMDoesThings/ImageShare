using Azure.Storage.Files.DataLake;

namespace ImageShare.Functions.Interfaces;

public interface IDataLakeServiceClientFactory
{
    DataLakeServiceClient Create(string connectionString);
}