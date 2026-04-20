using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEnergyModulePhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Energy_Locations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BuildingId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShortName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, defaultValue: "Dubai")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalUnits = table.Column<int>(type: "int", nullable: false),
                    Towers = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Floors = table.Column<int>(type: "int", nullable: false),
                    Basements = table.Column<int>(type: "int", nullable: false),
                    BmsType = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GatewayId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OccupancyPercent = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "onboarding")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DesignDeltaT = table.Column<decimal>(type: "decimal(4,1)", nullable: false, defaultValue: 5.5m),
                    ConnectedSince = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastDataReceived = table.Column<DateTime>(type: "datetime", nullable: true),
                    MqttTopicPrefix = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InfluxBucket = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Energy_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Energy_Locations_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "BuildingId",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Energy_Alerts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LocationId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Severity = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    System = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Equipment = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Timestamp = table.Column<DateTime>(type: "datetime", nullable: false),
                    Acknowledged = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AcknowledgedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    EventCount = table.Column<int>(type: "int", nullable: false),
                    BmsReference = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecommendedAction = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstimatedSavingsAedMonth = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Energy_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Energy_Alerts_Energy_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Energy_Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Energy_DewaMeters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LocationId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PremiseLabel = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MeterNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MultiplicationFactor = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CtRatio = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasWater = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Energy_DewaMeters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Energy_DewaMeters_Energy_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Energy_Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Energy_DewaBills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LocationId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MeterId = table.Column<int>(type: "int", nullable: false),
                    BillMonth = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PeriodStart = table.Column<DateTime>(type: "date", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "date", nullable: true),
                    Kwh = table.Column<int>(type: "int", nullable: false),
                    ElectricityAed = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    WaterCubicMeters = table.Column<decimal>(type: "decimal(10,3)", nullable: true),
                    WaterAed = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    SewerageAed = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    MeterReadingPrevious = table.Column<int>(type: "int", nullable: true),
                    MeterReadingCurrent = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Energy_DewaBills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Energy_DewaBills_Energy_DewaMeters_MeterId",
                        column: x => x.MeterId,
                        principalTable: "Energy_DewaMeters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Energy_DewaBills_Energy_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Energy_Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Energy_Alerts_LocationId_IsActive",
                table: "Energy_Alerts",
                columns: new[] { "LocationId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Energy_DewaBills_LocationId",
                table: "Energy_DewaBills",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Energy_DewaBills_LocationId_BillMonth",
                table: "Energy_DewaBills",
                columns: new[] { "LocationId", "BillMonth" });

            migrationBuilder.CreateIndex(
                name: "IX_Energy_DewaBills_MeterId",
                table: "Energy_DewaBills",
                column: "MeterId");

            migrationBuilder.CreateIndex(
                name: "IX_Energy_DewaMeters_AccountNumber",
                table: "Energy_DewaMeters",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Energy_DewaMeters_LocationId",
                table: "Energy_DewaMeters",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Energy_Locations_BuildingId",
                table: "Energy_Locations",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_Energy_Locations_Name",
                table: "Energy_Locations",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Energy_Alerts");

            migrationBuilder.DropTable(
                name: "Energy_DewaBills");

            migrationBuilder.DropTable(
                name: "Energy_DewaMeters");

            migrationBuilder.DropTable(
                name: "Energy_Locations");
        }
    }
}
