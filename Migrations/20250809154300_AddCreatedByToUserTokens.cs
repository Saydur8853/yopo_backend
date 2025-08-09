using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToUserTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "UserTokens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Update existing UserTokens to set CreatedBy = UserId (tokens are created by users themselves)
            migrationBuilder.Sql("UPDATE UserTokens SET CreatedBy = UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserTokens");
        }
    }
}
