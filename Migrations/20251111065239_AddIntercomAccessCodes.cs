using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddIntercomAccessCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntercomAccessCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BuildingId = table.Column<int>(type: "int", nullable: false),
                    IntercomId = table.Column<int>(type: "int", nullable: true),
                    CodeType = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodeHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    BuildingId1 = table.Column<int>(type: "int", nullable: false),
                    IntercomId1 = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntercomAccessCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntercomAccessCodes_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "BuildingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntercomAccessCodes_Buildings_BuildingId1",
                        column: x => x.BuildingId1,
                        principalTable: "Buildings",
                        principalColumn: "BuildingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntercomAccessCodes_Intercoms_IntercomId",
                        column: x => x.IntercomId,
                        principalTable: "Intercoms",
                        principalColumn: "IntercomId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_IntercomAccessCodes_Intercoms_IntercomId1",
                        column: x => x.IntercomId1,
                        principalTable: "Intercoms",
                        principalColumn: "IntercomId");
                    table.ForeignKey(
                        name: "FK_IntercomAccessCodes_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IntercomAccessCodes_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_IntercomAccessCodes_BuildingId",
                table: "IntercomAccessCodes",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_IntercomAccessCodes_BuildingId_IntercomId_IsActive",
                table: "IntercomAccessCodes",
                columns: new[] { "BuildingId", "IntercomId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_IntercomAccessCodes_BuildingId1",
                table: "IntercomAccessCodes",
                column: "BuildingId1");

            migrationBuilder.CreateIndex(
                name: "IX_IntercomAccessCodes_CreatedBy",
                table: "IntercomAccessCodes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_IntercomAccessCodes_CreatedByUserId",
                table: "IntercomAccessCodes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IntercomAccessCodes_IntercomId",
                table: "IntercomAccessCodes",
                column: "IntercomId");

            migrationBuilder.CreateIndex(
                name: "IX_IntercomAccessCodes_IntercomId1",
                table: "IntercomAccessCodes",
                column: "IntercomId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntercomAccessCodes");
        }
    }
}
