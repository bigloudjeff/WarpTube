using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using WarpTube.Shared.Services;
using WarpTube.Services;
using WarpTube.Shared.Data;

namespace WarpTube;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Configure Entity Framework and SQLite
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "warptube.db");
        builder.Services.AddDbContextFactory<WarpTubeDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Add device-specific services used by the WarpTube.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        
        // Register YouTube download service
        builder.Services.AddScoped<IYouTubeDownloadService, YouTubeDownloadService>();

        builder.Services.AddMauiBlazorWebView();
        
        // Register pages
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
