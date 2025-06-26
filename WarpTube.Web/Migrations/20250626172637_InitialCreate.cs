using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarpTube.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VideoId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: false),
                    Channel = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ChannelId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    ViewCount = table.Column<long>(type: "INTEGER", nullable: true),
                    LikeCount = table.Column<long>(type: "INTEGER", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    OriginalUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    LocalFilePath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Format = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Resolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: true),
                    AudioCodec = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    VideoCodec = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Fps = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Videos_VideoId",
                table: "Videos",
                column: "VideoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Videos");
        }
    }
}
