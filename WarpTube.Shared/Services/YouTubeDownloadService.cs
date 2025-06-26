using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using WarpTube.Shared.Data;
using WarpTube.Shared.Models;

namespace WarpTube.Shared.Services;

public class YouTubeDownloadService : IYouTubeDownloadService
{
    private readonly IDbContextFactory<WarpTubeDbContext> _dbContextFactory;

    public YouTubeDownloadService(IDbContextFactory<WarpTubeDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Video?> GetVideoInfoAsync(string url)
    {
        if (!IsValidYouTubeUrl(url))
            return null;

        var videoId = ExtractVideoIdFromUrl(url);
        if (string.IsNullOrEmpty(videoId))
            return null;

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var existingVideo = await dbContext.Videos.FirstOrDefaultAsync(v => v.VideoId == videoId);
        
        if (existingVideo != null)
        {
            existingVideo.OriginalUrl = url;
            existingVideo.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
            return existingVideo;
        }

        // Use yt-dlp to fetch video metadata
        var startInfo = new ProcessStartInfo
        {
            FileName = "yt-dlp",
            Arguments = $"--dump-json --no-download -- \"{url}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
            return null;

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"yt-dlp failed: {error}");
        }

        try
        {
            // Parse JSON output to extract video information
            using var jsonDoc = System.Text.Json.JsonDocument.Parse(output);
            var root = jsonDoc.RootElement;

            var video = new Video
            {
                VideoId = videoId,
                Title = root.GetProperty("title").GetString() ?? "Unknown Title",
                Description = root.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                Channel = root.TryGetProperty("uploader", out var uploader) ? uploader.GetString() ?? "" : "",
                ChannelId = root.TryGetProperty("uploader_id", out var uploaderId) ? uploaderId.GetString() ?? "" : "",
                OriginalUrl = url,
                Status = VideoStatus.Active
            };

            // Parse published date
            if (root.TryGetProperty("upload_date", out var uploadDate) && uploadDate.GetString() is string dateStr)
            {
                if (DateTime.TryParseExact(dateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    video.PublishedAt = parsedDate;
                }
            }

            // Parse duration
            if (root.TryGetProperty("duration", out var durationElement) && durationElement.TryGetDouble(out var durationSeconds))
            {
                video.Duration = TimeSpan.FromSeconds(durationSeconds);
            }

            // Parse view count
            if (root.TryGetProperty("view_count", out var viewCount) && viewCount.TryGetInt64(out var views))
            {
                video.ViewCount = views;
            }

            // Parse like count
            if (root.TryGetProperty("like_count", out var likeCount) && likeCount.TryGetInt64(out var likes))
            {
                video.LikeCount = likes;
            }

            // Parse thumbnail URL
            if (root.TryGetProperty("thumbnail", out var thumbnail))
            {
                video.ThumbnailUrl = thumbnail.GetString() ?? "";
            }

            await dbContext.Videos.AddAsync(video);
            await dbContext.SaveChangesAsync();
            return video;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to parse video metadata: {ex.Message}");
        }
    }


    public async Task<bool> IsValidYouTubeUrlAsync(string url)
    {
        return IsValidYouTubeUrl(url);
    }

    private bool IsValidYouTubeUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Host.Contains("youtube.com") || uriResult.Host.Contains("youtu.be"));
    }

    public string ExtractVideoIdFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            
            // Handle youtu.be URLs
            if (uri.Host == "youtu.be")
            {
                return uri.Segments.Last().Replace("/", "");
            }
            
            // Handle youtube.com URLs
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query.Get("v") ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
