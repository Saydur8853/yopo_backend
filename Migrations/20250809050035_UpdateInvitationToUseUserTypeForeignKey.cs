using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInvitationToUseUserTypeForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserRoll",
                table: "Invitations");

            migrationBuilder.AddColumn<int>(
                name: "UserTypeId",
                table: "Invitations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_UserTypeId",
                table: "Invitations",
                column: "UserTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_UserTypes_UserTypeId",
                table: "Invitations",
                column: "UserTypeId",
                principalTable: "UserTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_UserTypes_UserTypeId",
                table: "Invitations");

            migrationBuilder.DropIndex(
                name: "IX_Invitations_UserTypeId",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "UserTypeId",
                table: "Invitations");

            migrationBuilder.AddColumn<string>(
                name: "UserRoll",
                table: "Invitations",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
