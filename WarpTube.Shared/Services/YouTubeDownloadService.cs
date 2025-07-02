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

        // If this is a channel URL, get the first video from the channel
        if (IsChannelUrl(url))
        {
            var videos = await GetVideosFromChannelAsync(url, 1);
            return videos.FirstOrDefault();
        }

        var videoId = ExtractVideoIdFromUrl(url);
        if (string.IsNullOrEmpty(videoId))
            return null;

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var existingVideo = await dbContext.Videos.FirstOrDefaultAsync(v => v.VideoId == videoId);
        
        if (existingVideo != null)
        {
            existingVideo.OriginalUrl = url;
            await dbContext.SaveChangesAsync();
            return existingVideo;
        }

        // Use yt-dlp to fetch video metadata
        var ytDlpPath = GetYtDlpPath();
        var startInfo = new ProcessStartInfo
        {
            FileName = ytDlpPath,
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
                Description = root.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                Channel = root.TryGetProperty("uploader", out var uploader) ? uploader.GetString() : null,
                ChannelId = root.TryGetProperty("uploader_id", out var uploaderId) ? uploaderId.GetString() : null,
                OriginalUrl = url,
                Status = VideoStatus.Active,
                CreatedAt = DateTime.UtcNow
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
                video.ThumbnailUrl = thumbnail.GetString();
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

    public async Task<List<Video>> GetVideosFromChannelAsync(string channelUrl, int maxVideos = 10)
    {
        if (!IsChannelUrl(channelUrl))
            return new List<Video>();

        var videos = new List<Video>();
        
        try
        {
            // Use yt-dlp to fetch video list from channel
            var ytDlpPath = GetYtDlpPath();
            var startInfo = new ProcessStartInfo
            {
                FileName = ytDlpPath,
                Arguments = $"--dump-json --flat-playlist --playlist-end {maxVideos} -- \"{channelUrl}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
                return videos;

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"yt-dlp failed to fetch channel videos: {error}");
            }

            // Parse each line as a separate JSON object (yt-dlp outputs one JSON per line in flat-playlist mode)
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            
            foreach (var line in lines.Take(maxVideos))
            {
                try
                {
                    using var jsonDoc = System.Text.Json.JsonDocument.Parse(line);
                    var root = jsonDoc.RootElement;
                    
                    // Extract video ID
                    var videoId = root.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                    if (string.IsNullOrEmpty(videoId))
                        continue;

                    // Check if we already have this video in the database
                    var existingVideo = await dbContext.Videos.FirstOrDefaultAsync(v => v.VideoId == videoId);
                    if (existingVideo != null)
                    {
                        videos.Add(existingVideo);
                        continue;
                    }

                    // Create new video from channel listing
                    var video = new Video
                    {
                        VideoId = videoId,
                        Title = root.TryGetProperty("title", out var title) ? title.GetString() ?? "Unknown Title" : "Unknown Title",
                        Description = root.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                        Channel = root.TryGetProperty("uploader", out var uploader) ? uploader.GetString() : null,
                        ChannelId = root.TryGetProperty("uploader_id", out var uploaderId) ? uploaderId.GetString() : null,
                        OriginalUrl = $"https://www.youtube.com/watch?v={videoId}",
                        Status = VideoStatus.Active,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Parse duration if available
                    if (root.TryGetProperty("duration", out var durationElement) && durationElement.TryGetDouble(out var durationSeconds))
                    {
                        video.Duration = TimeSpan.FromSeconds(durationSeconds);
                    }

                    // Parse view count if available
                    if (root.TryGetProperty("view_count", out var viewCount) && viewCount.TryGetInt64(out var views))
                    {
                        video.ViewCount = views;
                    }

                    // Parse upload date if available
                    if (root.TryGetProperty("upload_date", out var uploadDate) && uploadDate.GetString() is string dateStr)
                    {
                        if (DateTime.TryParseExact(dateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                        {
                            video.PublishedAt = parsedDate;
                        }
                    }

                    // Parse thumbnail URL if available
                    if (root.TryGetProperty("thumbnail", out var thumbnail))
                    {
                        video.ThumbnailUrl = thumbnail.GetString();
                    }
                    else if (root.TryGetProperty("thumbnails", out var thumbnails) && thumbnails.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        // Get the last (highest quality) thumbnail
                        var thumbnailArray = thumbnails.EnumerateArray().ToArray();
                        if (thumbnailArray.Length > 0)
                        {
                            var lastThumbnail = thumbnailArray.Last();
                            if (lastThumbnail.TryGetProperty("url", out var thumbnailUrl))
                            {
                                video.ThumbnailUrl = thumbnailUrl.GetString();
                            }
                        }
                    }

                    await dbContext.Videos.AddAsync(video);
                    videos.Add(video);
                }
                catch (Exception ex)
                {
                    // Log error but continue processing other videos
                    Console.WriteLine($"Error parsing video from channel: {ex.Message}");
                    continue;
                }
            }
            
            await dbContext.SaveChangesAsync();
            return videos;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to fetch videos from channel: {ex.Message}");
        }
    }

    private bool IsValidYouTubeUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Host.Contains("youtube.com") || uriResult.Host.Contains("youtu.be"));
    }

    public bool IsChannelUrl(string url)
    {
        if (!IsValidYouTubeUrl(url))
            return false;

        try
        {
            var uri = new Uri(url);
            
            // Check for channel URLs: youtube.com/@channel, youtube.com/channel/UCxxx, youtube.com/c/channel, youtube.com/user/xxx
            if (uri.Host.Contains("youtube.com"))
            {
                var path = uri.AbsolutePath.ToLower();
                return path.StartsWith("/@") || 
                       path.StartsWith("/channel/") || 
                       path.StartsWith("/c/") || 
                       path.StartsWith("/user/");
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool IsVideoUrl(string url)
    {
        if (!IsValidYouTubeUrl(url))
            return false;

        try
        {
            var uri = new Uri(url);
            
            // Handle youtu.be URLs (always video URLs)
            if (uri.Host == "youtu.be")
            {
                return true;
            }
            
            // Handle youtube.com URLs with watch parameter
            if (uri.Host.Contains("youtube.com"))
            {
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                return !string.IsNullOrEmpty(query.Get("v"));
            }
            
            return false;
        }
        catch
        {
            return false;
        }
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

    private string GetYtDlpPath()
    {
        // Try local tools directory first
        var localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "yt-dlp");
        if (File.Exists(localPath))
        {
            return localPath;
        }

        // Try project tools directory
        var projectToolsPath = Path.Combine(Directory.GetCurrentDirectory(), "tools", "yt-dlp");
        if (File.Exists(projectToolsPath))
        {
            return projectToolsPath;
        }

        // Fall back to system installation
        return "/opt/homebrew/bin/yt-dlp";
    }
}
