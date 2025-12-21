using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
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
