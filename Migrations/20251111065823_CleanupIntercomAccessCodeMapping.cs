using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class CleanupIntercomAccessCodeMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FKs that reference shadow columns
            migrationBuilder.DropForeignKey(
                name: "FK_IntercomAccessCodes_Buildings_BuildingId1",
                table: "IntercomAccessCodes");
            migrationBuilder.DropForeignKey(
                name: "FK_IntercomAccessCodes_Intercoms_IntercomId1",
                table: "IntercomAccessCodes");
            migrationBuilder.DropForeignKey(
                name: "FK_IntercomAccessCodes_Users_CreatedByUserId",
                table: "IntercomAccessCodes");

            // Drop indexes on shadow columns
            migrationBuilder.DropIndex(
                name: "IX_IntercomAccessCodes_BuildingId1",
                table: "IntercomAccessCodes");
            migrationBuilder.DropIndex(
                name: "IX_IntercomAccessCodes_IntercomId1",
                table: "IntercomAccessCodes");
            migrationBuilder.DropIndex(
                name: "IX_IntercomAccessCodes_CreatedByUserId",
                table: "IntercomAccessCodes");

            // Drop shadow columns
            migrationBuilder.DropColumn(
                name: "BuildingId1",
                table: "IntercomAccessCodes");
            migrationBuilder.DropColumn(
                name: "IntercomId1",
                table: "IntercomAccessCodes");
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "IntercomAccessCodes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate columns
            migrationBuilder.AddColumn<int>(
                name: "BuildingId1",
                table: "IntercomAccessCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IntercomId1",
                table: "IntercomAccessCodes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "IntercomAccessCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Recreate indexes
            migrationBuilder.CreateIndex(
                name: "IX_IntercomAccessCodes_BuildingId1",
                table: "IntercomAccessCodes",
                column: "BuildingId1");

            migrationBuilder.CreateIndex(
                name: "IX_IntercomAccessCodes_IntercomId1",
                table: "IntercomAccessCodes",
                column: "IntercomId1");

            migrationBuilder.CreateIndex(
                name: "IX_IntercomAccessCodes_CreatedByUserId",
                table: "IntercomAccessCodes",
                column: "CreatedByUserId");

            // Recreate FKs
            migrationBuilder.AddForeignKey(
                name: "FK_IntercomAccessCodes_Buildings_BuildingId1",
                table: "IntercomAccessCodes",
                column: "BuildingId1",
                principalTable: "Buildings",
                principalColumn: "BuildingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IntercomAccessCodes_Intercoms_IntercomId1",
                table: "IntercomAccessCodes",
                column: "IntercomId1",
                principalTable: "Intercoms",
                principalColumn: "IntercomId");

            migrationBuilder.AddForeignKey(
                name: "FK_IntercomAccessCodes_Users_CreatedByUserId",
                table: "IntercomAccessCodes",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
