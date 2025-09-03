using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class CreateNotificationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SentTo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SendFrom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BuildingId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    WarningLevel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    File = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ScheduledAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    RequiresAcknowledgment = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsUrgent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SendEmail = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SendSMS = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SendPush = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SendInApp = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Metadata = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientCount = table.Column<int>(type: "int", nullable: false),
                    ReadCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_BuildingId",
                table: "Notifications",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedBy",
                table: "Notifications",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CustomerId",
                table: "Notifications",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsActive",
                table: "Notifications",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsUrgent",
                table: "Notifications",
                column: "IsUrgent");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Priority",
                table: "Notifications",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ScheduledAt",
                table: "Notifications",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TenantId",
                table: "Notifications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type_Status",
                table: "Notifications",
                columns: new[] { "Type", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
