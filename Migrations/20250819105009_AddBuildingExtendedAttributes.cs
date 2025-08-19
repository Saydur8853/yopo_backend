using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingExtendedAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Buildings",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CommercialUnit",
                table: "Buildings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateStartOperation",
                table: "Buildings",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Developer",
                table: "Buildings",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Floors",
                table: "Buildings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasGym",
                table: "Buildings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasReception",
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

            migrationBuilder.AddColumn<bool>(
                name: "HasSwimpool",
                table: "Buildings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ParkingFloor",
                table: "Buildings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParkingSpace",
                table: "Buildings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Buildings",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Units",
                table: "Buildings",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "CommercialUnit",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "DateStartOperation",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "Developer",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "Floors",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "HasGym",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "HasReception",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "HasSauna",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "HasSwimpool",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "ParkingFloor",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "ParkingSpace",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "Units",
                table: "Buildings");
        }
    }
}
