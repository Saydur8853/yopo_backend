using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddThreadSocial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_BuildingId",
                table: "Messages");

            migrationBuilder.CreateTable(
                name: "ThreadPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    AuthorType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Image = table.Column<byte[]>(type: "LONGBLOB", nullable: true),
                    ImageMimeType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BuildingId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreadPosts", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ThreadComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    AuthorType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreadComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThreadComments_ThreadPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "ThreadPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ThreadComments_AuthorId",
                table: "ThreadComments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ThreadComments_PostId",
                table: "ThreadComments",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_ThreadPosts_AuthorId",
                table: "ThreadPosts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ThreadPosts_BuildingId",
                table: "ThreadPosts",
                column: "BuildingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThreadComments");

            migrationBuilder.DropTable(
                name: "ThreadPosts");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_BuildingId",
                table: "Messages",
                column: "BuildingId");
        }
    }
}
