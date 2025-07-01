# CLAUDE.md - WarpTube

## Project Overview

A YouTube downloader application with both MAUI mobile/desktop interface and web interface, featuring video download capabilities using yt-dlp integration.

## Technology Stack

- **Framework**: .NET MAUI with Blazor Hybrid + separate Blazor Server web app
- **Target Framework**: .NET 9.0
- **Database**: SQLite with Entity Framework Core
- **External APIs**: Google YouTube API v3
- **Download Engine**: yt-dlp (external tool)
- **Architecture**: Shared component library pattern

## Project Structure

- `WarpTube/` - MAUI application project
- `WarpTube.Web/` - Blazor Server web application
- `WarpTube.Shared/` - Shared Blazor components and services
- `tools/yt-dlp` - YouTube download utility
- `videos/` - Downloaded video storage directory

## Database

- **Provider**: SQLite via Entity Framework Core
- **Context**: WarpTubeDbContext
- **Migrations**: EF Core migrations for schema management
- **Models**: Video entity for tracking downloads

## Common Commands

### Building and Running

```bash
# Build entire solution
dotnet build WarpTube.sln

# Run web version (recommended for development)
./run-web.sh
# or
dotnet run --project WarpTube.Web/WarpTube.Web.csproj

# Run MAUI version on macOS
./run-mac.sh

# Generic run script
./run.sh
```

### Database Operations

```bash
# Add migration (from WarpTube.Web directory)
dotnet ef migrations add [MigrationName]

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### Platform-Specific Commands

```bash
# Build MAUI for specific platforms
dotnet build WarpTube/WarpTube.csproj -f net9.0-android
dotnet build WarpTube/WarpTube.csproj -f net9.0-ios
dotnet build WarpTube/WarpTube.csproj -f net9.0-maccatalyst
```

### Testing

No test projects are currently configured.

## Key Features

- **YouTube Integration**: Search and download videos using YouTube API
- **Cross-Platform**: MAUI app for mobile/desktop, web app for browsers
- **Database Tracking**: SQLite database for tracking downloaded videos
- **Shared Components**: Reusable Blazor components across platforms
- **File Management**: Organized video storage and management

## External Dependencies

- **yt-dlp**: Required for video downloading functionality
- **YouTube API Key**: Required for YouTube search functionality
- **FFmpeg**: May be required for video processing (check yt-dlp requirements)

## Configuration

- **YouTube API**: Configure API key in application settings
- **Download Path**: Configure video download directory
- **Database**: SQLite database file location

## Development Notes

- Uses shared Blazor components between MAUI and web versions
- YouTube API integration for search functionality
- yt-dlp handles actual video downloading
- EF Core tracks download history and metadata
- Form factor service differentiates between platforms

## Deployment Requirements

- yt-dlp tool must be available in tools directory
- YouTube API credentials properly configured
- Sufficient storage space for video downloads
- Network access for YouTube API and video downloads

## Next Steps

- Add comprehensive error handling for downloads
- Implement download progress tracking
- Add video format selection options
- Create unit tests for download services
- Add download queue management
- Implement user authentication and personalization