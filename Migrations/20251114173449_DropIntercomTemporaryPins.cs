using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class DropIntercomTemporaryPins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntercomTemporaryPinUsages");

            migrationBuilder.DropTable(
                name: "IntercomTemporaryPins");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntercomTemporaryPins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    IntercomId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    FirstUsedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    MaxUses = table.Column<int>(type: "int", nullable: false),
                    PinHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsesCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntercomTemporaryPins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntercomTemporaryPins_Intercoms_IntercomId",
                        column: x => x.IntercomId,
                        principalTable: "Intercoms",
                        principalColumn: "IntercomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntercomTemporaryPins_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "IntercomTemporaryPinUsages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TemporaryPinId = table.Column<int>(type: "int", nullable: false),
                    DeviceInfo = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UsedFromIp = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntercomTemporaryPinUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntercomTemporaryPinUsages_IntercomTemporaryPins_TemporaryPi~",
                        column: x => x.TemporaryPinId,
                        principalTable: "IntercomTemporaryPins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_IntercomTemporaryPins_CreatedByUserId",
                table: "IntercomTemporaryPins",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IntercomTemporaryPins_IntercomId_IsActive",
                table: "IntercomTemporaryPins",
                columns: new[] { "IntercomId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_IntercomTemporaryPinUsages_TemporaryPinId",
                table: "IntercomTemporaryPinUsages",
                column: "TemporaryPinId");
        }
    }
}
