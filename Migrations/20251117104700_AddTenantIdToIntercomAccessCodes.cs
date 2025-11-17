using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToIntercomAccessCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "IntercomAccessCodes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntercomAccessCodes_TenantId",
                table: "IntercomAccessCodes",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_IntercomAccessCodes_Tenants_TenantId",
                table: "IntercomAccessCodes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntercomAccessCodes_Tenants_TenantId",
                table: "IntercomAccessCodes");

            migrationBuilder.DropIndex(
                name: "IX_IntercomAccessCodes_TenantId",
                table: "IntercomAccessCodes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "IntercomAccessCodes");
        }
    }
}
