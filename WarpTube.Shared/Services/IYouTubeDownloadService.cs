using WarpTube.Shared.Models;

namespace WarpTube.Shared.Services;

public interface IYouTubeDownloadService
{
    Task<Video?> GetVideoInfoAsync(string url);
    Task<bool> IsValidYouTubeUrlAsync(string url);
    string ExtractVideoIdFromUrl(string url);
}
