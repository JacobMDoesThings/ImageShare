using ImageShare.WebApi.Configuration;
using ImageShare.WebApi.Data;

namespace ImageShare.WebApi.Services;

public interface ISasGeneratorService
{
    public Task<string> GenerateWriteSasTokenAsync(UserImageInfo userImageInfo);
    public Task<IEnumerable<string>> BulkGenerateWriteSasUrlAsync(UserImageInfo userImageInfo, int count);
}