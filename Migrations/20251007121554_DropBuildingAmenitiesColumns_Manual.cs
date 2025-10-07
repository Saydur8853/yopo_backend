using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class DropBuildingAmenitiesColumns_Manual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Manually drop columns that were removed from the Building model but not dropped by the previous empty migration
            migrationBuilder.DropColumn(
                name: "Floors",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "ParkingFloor",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "HasGym",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "HasSwimmingPool",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "HasSauna",
                table: "Buildings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate the columns if this migration is rolled back
            migrationBuilder.AddColumn<int>(
                name: "Floors",
                table: "Buildings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParkingFloor",
                table: "Buildings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasGym",
                table: "Buildings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasSwimmingPool",
                table: "Buildings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasSauna",
                table: "Buildings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
