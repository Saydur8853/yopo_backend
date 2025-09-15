using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserProfilePhotoToBinary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update ProfilePhoto column to LONGBLOB for binary image storage
            migrationBuilder.AlterColumn<byte[]>(
                name: "ProfilePhoto",
                table: "Users",
                type: "LONGBLOB",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            // Add ProfilePhotoMimeType column to store image MIME type
            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoMimeType",
                table: "Users",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove ProfilePhotoMimeType column
            migrationBuilder.DropColumn(
                name: "ProfilePhotoMimeType",
                table: "Users");

            // Revert ProfilePhoto column back to VARCHAR
            migrationBuilder.AlterColumn<string>(
                name: "ProfilePhoto",
                table: "Users",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "LONGBLOB",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
