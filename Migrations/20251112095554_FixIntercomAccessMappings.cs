using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class FixIntercomAccessMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only clean up IntercomAccessCodes duplicates. Other tables were created via raw SQL without duplicates.
            // Use conditional SQL to avoid errors if the constraints/columns were already dropped.
            migrationBuilder.Sql(@"
                SET @fk := (SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE
                            WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'IntercomAccessCodes' AND CONSTRAINT_NAME = 'FK_IntercomAccessCodes_Buildings_BuildingId1');
                SET @sql := IF(@fk IS NOT NULL, 'ALTER TABLE `IntercomAccessCodes` DROP FOREIGN KEY `FK_IntercomAccessCodes_Buildings_BuildingId1`;', 'SELECT 1;');
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

                SET @fk := (SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE
                            WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'IntercomAccessCodes' AND CONSTRAINT_NAME = 'FK_IntercomAccessCodes_Intercoms_IntercomId1');
                SET @sql := IF(@fk IS NOT NULL, 'ALTER TABLE `IntercomAccessCodes` DROP FOREIGN KEY `FK_IntercomAccessCodes_Intercoms_IntercomId1`;', 'SELECT 1;');
                PREPARE stmt2 FROM @sql; EXECUTE stmt2; DEALLOCATE PREPARE stmt2;

                SET @fk := (SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE
                            WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'IntercomAccessCodes' AND CONSTRAINT_NAME = 'FK_IntercomAccessCodes_Users_CreatedByUserId');
                SET @sql := IF(@fk IS NOT NULL, 'ALTER TABLE `IntercomAccessCodes` DROP FOREIGN KEY `FK_IntercomAccessCodes_Users_CreatedByUserId`;', 'SELECT 1;');
                PREPARE stmt3 FROM @sql; EXECUTE stmt3; DEALLOCATE PREPARE stmt3;

                SET @idx := (SELECT INDEX_NAME FROM information_schema.STATISTICS
                             WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'IntercomAccessCodes' AND INDEX_NAME = 'IX_IntercomAccessCodes_BuildingId1');
                SET @sql := IF(@idx IS NOT NULL, 'DROP INDEX `IX_IntercomAccessCodes_BuildingId1` ON `IntercomAccessCodes`;', 'SELECT 1;');
                PREPARE stmt4 FROM @sql; EXECUTE stmt4; DEALLOCATE PREPARE stmt4;

                SET @idx := (SELECT INDEX_NAME FROM information_schema.STATISTICS
                             WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'IntercomAccessCodes' AND INDEX_NAME = 'IX_IntercomAccessCodes_IntercomId1');
                SET @sql := IF(@idx IS NOT NULL, 'DROP INDEX `IX_IntercomAccessCodes_IntercomId1` ON `IntercomAccessCodes`;', 'SELECT 1;');
                PREPARE stmt5 FROM @sql; EXECUTE stmt5; DEALLOCATE PREPARE stmt5;

                SET @idx := (SELECT INDEX_NAME FROM information_schema.STATISTICS
                             WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'IntercomAccessCodes' AND INDEX_NAME = 'IX_IntercomAccessCodes_CreatedByUserId');
                SET @sql := IF(@idx IS NOT NULL, 'DROP INDEX `IX_IntercomAccessCodes_CreatedByUserId` ON `IntercomAccessCodes`;', 'SELECT 1;');
                PREPARE stmt6 FROM @sql; EXECUTE stmt6; DEALLOCATE PREPARE stmt6;

                SET @col := (SELECT COLUMN_NAME FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'IntercomAccessCodes' AND COLUMN_NAME = 'BuildingId1');
                SET @sql := IF(@col IS NOT NULL, 'ALTER TABLE `IntercomAccessCodes` DROP COLUMN `BuildingId1`;', 'SELECT 1;');
                PREPARE stmt7 FROM @sql; EXECUTE stmt7; DEALLOCATE PREPARE stmt7;

                SET @col := (SELECT COLUMN_NAME FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'IntercomAccessCodes' AND COLUMN_NAME = 'IntercomId1');
                SET @sql := IF(@col IS NOT NULL, 'ALTER TABLE `IntercomAccessCodes` DROP COLUMN `IntercomId1`;', 'SELECT 1;');
                PREPARE stmt8 FROM @sql; EXECUTE stmt8; DEALLOCATE PREPARE stmt8;

                SET @col := (SELECT COLUMN_NAME FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'IntercomAccessCodes' AND COLUMN_NAME = 'CreatedByUserId');
                SET @sql := IF(@col IS NOT NULL, 'ALTER TABLE `IntercomAccessCodes` DROP COLUMN `CreatedByUserId`;', 'SELECT 1;');
                PREPARE stmt9 FROM @sql; EXECUTE stmt9; DEALLOCATE PREPARE stmt9;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate only the IntercomAccessCodes shadow columns if needed (not recommended).
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
