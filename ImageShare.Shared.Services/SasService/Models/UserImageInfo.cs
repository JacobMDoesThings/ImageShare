namespace ImageShare.Shared.Services.SasService.Models;

public class UserImageInfo
{
    public required string UserId { get; set; }
    public required string[] GroupIds { get; set; }
    public required string SelectedGroupId { get; set; }
    public bool IsPublicImage { get; set; }
}