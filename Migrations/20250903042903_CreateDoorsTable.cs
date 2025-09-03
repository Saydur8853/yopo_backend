using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class CreateDoorsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Doors",
                columns: table => new
                {
                    DoorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BuildingId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IntercomId = table.Column<int>(type: "int", nullable: true),
                    CCTVId = table.Column<int>(type: "int", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FireExit = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PinOnly = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanOpenByWatchCommand = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsCarPark = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Location = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Floor = table.Column<int>(type: "int", nullable: true),
                    HasAutoLock = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AutoLockDelay = table.Column<int>(type: "int", nullable: true),
                    HasCardAccess = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HasBiometricAccess = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaxPinAttempts = table.Column<int>(type: "int", nullable: true),
                    LockoutDuration = table.Column<int>(type: "int", nullable: true),
                    AccessLevel = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OperatingHours = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsMonitored = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doors", x => x.DoorId);
                    table.ForeignKey(
                        name: "FK_Doors_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Doors_CCTVs_CCTVId",
                        column: x => x.CCTVId,
                        principalTable: "CCTVs",
                        principalColumn: "CctvId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Doors_Intercoms_IntercomId",
                        column: x => x.IntercomId,
                        principalTable: "Intercoms",
                        principalColumn: "IntercomId",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Doors_BuildingId",
                table: "Doors",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_Doors_BuildingId_Type_Location",
                table: "Doors",
                columns: new[] { "BuildingId", "Type", "Location" });

            migrationBuilder.CreateIndex(
                name: "IX_Doors_CCTVId",
                table: "Doors",
                column: "CCTVId");

            migrationBuilder.CreateIndex(
                name: "IX_Doors_IntercomId",
                table: "Doors",
                column: "IntercomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Doors");
        }
    }
}
