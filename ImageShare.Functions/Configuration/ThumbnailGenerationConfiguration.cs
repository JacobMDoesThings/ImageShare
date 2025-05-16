using System.ComponentModel.DataAnnotations;

namespace ImageShare.Functions.Configuration;

public class ThumbnailGenerationConfiguration
{
    [Required (ErrorMessage = "Max Height required")]
    [Range(1, 2048,  ErrorMessage = "Max Height must be between 1 and 2048")]
    public int MaxHeight { get; init; }

    [Required(ErrorMessage = "Max Width required")]
    [Range(1, 100,   ErrorMessage = "Max Width must be between 1 and 100")]
    public int JpegQuality { get; init; }

    [Required(ErrorMessage = "Must include Allowed Mime Types")]
    public required string[] AllowedMimeTypes { get; init; }

    [Required(ErrorMessage = "Thumbnail Path Public required")]
    public required string ThumbnailPathPublic { get; init; }
    
    [Required(ErrorMessage = "Thumbnail Path Private required")]
    public required string ThumbnailPathPrivate { get; init; }
    
    [Required(ErrorMessage = "ConnectionString required")]
    public required string ConnectionString { get; init; }
}