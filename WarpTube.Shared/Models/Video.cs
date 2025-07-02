using System.ComponentModel.DataAnnotations;

namespace WarpTube.Shared.Models;

public class Video
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string VideoId { get; set; } = string.Empty;
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string? Channel { get; set; }
    
    public string? ChannelId { get; set; }
    
    public DateTime? PublishedAt { get; set; }
    
    public TimeSpan? Duration { get; set; }
    
    public long? ViewCount { get; set; }
    
    public long? LikeCount { get; set; }
    
    public string? ThumbnailUrl { get; set; }
    
    public string OriginalUrl { get; set; } = string.Empty;
    
    public VideoStatus Status { get; set; } = VideoStatus.Active;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    // Additional metadata from yt-dlp
    public string? Format { get; set; }
    
    public string? Resolution { get; set; }
    
    public long? FileSizeBytes { get; set; }
    
    public string? AudioCodec { get; set; }
    
    public string? VideoCodec { get; set; }
    
    public double? Fps { get; set; }
}

public enum VideoStatus
{
    Active,
    Failed,
    Archived
}
