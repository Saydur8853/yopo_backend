using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddThreadPostImageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "ThreadPosts");

            migrationBuilder.DropColumn(
                name: "ImageMimeType",
                table: "ThreadPosts");

            migrationBuilder.AddColumn<string>(
                name: "ImagePublicId",
                table: "ThreadPosts",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ThreadPosts",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePublicId",
                table: "ThreadPosts");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ThreadPosts");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "ThreadPosts",
                type: "LONGBLOB",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageMimeType",
                table: "ThreadPosts",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
