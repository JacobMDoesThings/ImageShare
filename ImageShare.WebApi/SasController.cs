using ImageShare.Shared.Services.SasService;
using ImageShare.Shared.Services.SasService.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImageShare.WebApi;

[ApiController]
[Route("api/[controller]")]
public class SasController(ISasGeneratorService sasGeneratorService)
    : ControllerBase
{
    [HttpGet("generateWriteSas")]
    [RequestFormLimits(MultipartBodyLengthLimit = (10 * 1024 * 1024))]
    public async Task<IActionResult> GenerateSasTokenForUpload(
        [FromQuery] string selectedGroupId,
        [FromQuery] bool isPublic = false)
    {
        // For Testing
        UserImageInfo userImageInfo = new()
        {
            UserId = "InternalId",
            GroupIds = ["groupid1", "groupid2", "groupid3"],
            IsPublicImage = isPublic,
            SelectedGroupId = selectedGroupId
        };

        if (!userImageInfo.GroupIds.Contains(selectedGroupId))
        {
            return Problem("not okay man");
        }

        try
        {
            //var sas = await sasGeneratorService.GenerateWriteSasTokenAsync(userImageInfo);
            return Ok(new {SasUrl = await sasGeneratorService.GenerateWriteSasTokenAsync(userImageInfo)});
        }
        catch (Exception ex)
        {
            return Problem($"Error generating SAS token: {ex.Message}");
        }
    }
}