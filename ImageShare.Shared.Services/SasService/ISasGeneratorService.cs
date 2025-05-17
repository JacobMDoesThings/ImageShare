using ImageShare.Shared.Services.SasService.Models;

namespace ImageShare.Shared.Services.SasService;

public interface ISasGeneratorService
{
    public Task<string> GenerateWriteSasTokenAsync(UserImageInfo userImageInfo);
    public Task<IEnumerable<string>> BulkGenerateWriteSasUrlAsync(UserImageInfo userImageInfo, int count);
}