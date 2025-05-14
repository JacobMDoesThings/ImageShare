using System.ComponentModel.DataAnnotations;

namespace ImageShare.Functions.Configuration;

public class ThumbnailGenerationConfiguration
{
    [Required]
    [Range(1, 2048)]
    public int MaxHeight { get; init; }

    [Required]
    [Range(1, 100)]
    public int JpegQuality { get; init; }

    [Required]
    public required string[] AllowedMimeTypes { get; init; }

    [Required]
    public string ThumbnailPathPublic { get; init; } = "public/thumbs";
    
    [Required]
    public string ThumbnailPathPrivate { get; init; } = "{user}/thumbs";
    
    [Required]
    public required string ConnectionString { get; init; }
}