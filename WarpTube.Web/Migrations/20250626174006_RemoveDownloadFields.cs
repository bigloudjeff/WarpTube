using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarpTube.Web.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDownloadFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalFilePath",
                table: "Videos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocalFilePath",
                table: "Videos",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);
        }
    }
}
