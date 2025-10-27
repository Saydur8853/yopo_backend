using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantAllocationToInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BuildingId",
                table: "Invitations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FloorId",
                table: "Invitations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "Invitations",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuildingId",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "FloorId",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "Invitations");
        }
    }
}
