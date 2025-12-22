using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using YopoBackend.Data;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20251227120000_AddThreadCommentReplies")]
    public partial class AddThreadCommentReplies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentCommentId",
                table: "ThreadComments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThreadComments_ParentCommentId",
                table: "ThreadComments",
                column: "ParentCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ThreadComments_ThreadComments_ParentCommentId",
                table: "ThreadComments",
                column: "ParentCommentId",
                principalTable: "ThreadComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThreadComments_ThreadComments_ParentCommentId",
                table: "ThreadComments");

            migrationBuilder.DropIndex(
                name: "IX_ThreadComments_ParentCommentId",
                table: "ThreadComments");

            migrationBuilder.DropColumn(
                name: "ParentCommentId",
                table: "ThreadComments");
        }
    }
}
