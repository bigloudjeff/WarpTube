using WarpTube.Shared.Models;

namespace WarpTube.Shared.Services;

public interface IYouTubeDownloadService
{
    Task<Video?> GetVideoInfoAsync(string url);
    Task<List<Video>> GetVideosFromChannelAsync(string channelUrl, int maxVideos = 10);
    Task<bool> IsValidYouTubeUrlAsync(string url);
    string ExtractVideoIdFromUrl(string url);
    bool IsChannelUrl(string url);
    bool IsVideoUrl(string url);
}
