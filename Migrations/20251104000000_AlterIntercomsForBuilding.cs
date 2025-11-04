using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using YopoBackend.Data;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20251104000000_AlterIntercomsForBuilding")]
    public partial class AlterIntercomsForBuilding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Add new columns (nullable to allow backfill)
            // Use idempotent DDL to tolerate partial prior attempts (works with older MySQL/MariaDB)
            migrationBuilder.Sql(@"
                SET @col_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Intercoms' AND COLUMN_NAME = 'BuildingId'
                );
                SET @sql := IF(@col_exists = 0,
                    'ALTER TABLE `Intercoms` ADD COLUMN `BuildingId` int NULL;',
                    'SELECT 1');
                PREPARE ac1 FROM @sql; EXECUTE ac1; DEALLOCATE PREPARE ac1;
            ");

            migrationBuilder.Sql(@"
                SET @col_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Intercoms' AND COLUMN_NAME = 'AmenityId'
                );
                SET @sql := IF(@col_exists = 0,
                    'ALTER TABLE `Intercoms` ADD COLUMN `AmenityId` int NULL;',
                    'SELECT 1');
                PREPARE ac2 FROM @sql; EXECUTE ac2; DEALLOCATE PREPARE ac2;
            ");

            // 2) Make InstalledLocation required and update existing null values
            migrationBuilder.AlterColumn<string>(
                name: "InstalledLocation",
                table: "Intercoms",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "Main Entrance",
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            // Ensure BuildingId column is nullable (to safely clean bad values)
            migrationBuilder.Sql(@"
                SET @need_null := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Intercoms' AND COLUMN_NAME = 'BuildingId' AND IS_NULLABLE = 'NO'
                );
                SET @sql := IF(@need_null > 0,
                    'ALTER TABLE `Intercoms` MODIFY COLUMN `BuildingId` int NULL;',
                    'SELECT 1');
                PREPARE an FROM @sql; EXECUTE an; DEALLOCATE PREPARE an;
            ");

            // 3) Backfill BuildingId from UnitId where possible (only if UnitId still exists)
            migrationBuilder.Sql(@"
                SET @unit_col_exists = (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Intercoms' AND COLUMN_NAME = 'UnitId'
                );
                SET @sql := IF(@unit_col_exists > 0,
                    'UPDATE Intercoms I JOIN Units U ON I.UnitId = U.UnitId SET I.BuildingId = U.BuildingId WHERE I.BuildingId IS NULL;',
                    'SELECT 1');
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
            ");

            // 4) For remaining nulls, pick the first building for the same customer (if any exist at all)
            migrationBuilder.Sql(@"
                SET @has_buildings := (SELECT COUNT(*) FROM Buildings);
                SET @sql := IF(@has_buildings > 0,
                    'UPDATE Intercoms I SET I.BuildingId = (SELECT MIN(B.BuildingId) FROM Buildings B WHERE B.CustomerId = I.CustomerId) WHERE I.BuildingId IS NULL;',
                    'SELECT 1');
                PREPARE bf FROM @sql; EXECUTE bf; DEALLOCATE PREPARE bf;
            ");

            // Clean up any invalid BuildingId values (set to NULL) so FK can be created later
            migrationBuilder.Sql(@"
                UPDATE Intercoms I
                LEFT JOIN Buildings B ON I.BuildingId = B.BuildingId
                SET I.BuildingId = NULL
                WHERE I.BuildingId IS NOT NULL AND B.BuildingId IS NULL;
            ");

            // 5) Create indexes (idempotent)
            migrationBuilder.Sql(@"
                SET @idx_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Intercoms' AND INDEX_NAME = 'IX_Intercoms_BuildingId'
                );
                SET @sql := IF(@idx_exists = 0,
                    'CREATE INDEX `IX_Intercoms_BuildingId` ON `Intercoms` (`BuildingId`);',
                    'SELECT 1');
                PREPARE s1 FROM @sql; EXECUTE s1; DEALLOCATE PREPARE s1;
            ");

            migrationBuilder.Sql(@"
                SET @idx_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Intercoms' AND INDEX_NAME = 'IX_Intercoms_AmenityId'
                );
                SET @sql := IF(@idx_exists = 0,
                    'CREATE INDEX `IX_Intercoms_AmenityId` ON `Intercoms` (`AmenityId`);',
                    'SELECT 1');
                PREPARE s2 FROM @sql; EXECUTE s2; DEALLOCATE PREPARE s2;
            ");

            // If there are still NULL BuildingId values and buildings exist, backfill to a valid building
            migrationBuilder.Sql(@"
                SET @has_buildings := (SELECT COUNT(*) FROM Buildings);
                SET @sql := IF(@has_buildings > 0,
                    'UPDATE Intercoms I SET I.BuildingId = (SELECT COALESCE((SELECT MIN(B1.BuildingId) FROM Buildings B1 WHERE B1.CustomerId = I.CustomerId), (SELECT MIN(B2.BuildingId) FROM Buildings B2))) WHERE I.BuildingId IS NULL;',
                    'SELECT 1');
                PREPARE bf2 FROM @sql; EXECUTE bf2; DEALLOCATE PREPARE bf2;
            ");

            // 6) Add foreign keys (idempotent)
            migrationBuilder.Sql(@"
                SET @fk_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
                    WHERE CONSTRAINT_SCHEMA = DATABASE() AND CONSTRAINT_NAME = 'FK_Intercoms_Buildings_BuildingId'
                );
                SET @sql := IF(@fk_exists = 0,
                    'ALTER TABLE `Intercoms` ADD CONSTRAINT `FK_Intercoms_Buildings_BuildingId` FOREIGN KEY (`BuildingId`) REFERENCES `Buildings` (`BuildingId`) ON DELETE CASCADE;',
                    'SELECT 1');
                PREPARE f1 FROM @sql; EXECUTE f1; DEALLOCATE PREPARE f1;
            ");

            migrationBuilder.Sql(@"
                SET @fk_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
                    WHERE CONSTRAINT_SCHEMA = DATABASE() AND CONSTRAINT_NAME = 'FK_Intercoms_Amenities_AmenityId'
                );
                SET @sql := IF(@fk_exists = 0,
                    'ALTER TABLE `Intercoms` ADD CONSTRAINT `FK_Intercoms_Amenities_AmenityId` FOREIGN KEY (`AmenityId`) REFERENCES `Amenities` (`AmenityId`) ON DELETE SET NULL;',
                    'SELECT 1');
                PREPARE f2 FROM @sql; EXECUTE f2; DEALLOCATE PREPARE f2;
            ");

            // 7) Drop UnitId FK, index and column if they still exist
            migrationBuilder.Sql(@"
                SET @fk_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
                    WHERE CONSTRAINT_SCHEMA = DATABASE() AND CONSTRAINT_NAME = 'FK_Intercoms_Units_UnitId'
                );
                SET @sql := IF(@fk_exists > 0,
                    'ALTER TABLE `Intercoms` DROP FOREIGN KEY `FK_Intercoms_Units_UnitId`;',
                    'SELECT 1');
                PREPARE d1 FROM @sql; EXECUTE d1; DEALLOCATE PREPARE d1;
            ");

            migrationBuilder.Sql(@"
                SET @idx_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Intercoms' AND INDEX_NAME = 'IX_Intercoms_UnitId'
                );
                SET @sql := IF(@idx_exists > 0,
                    'DROP INDEX `IX_Intercoms_UnitId` ON `Intercoms`;',
                    'SELECT 1');
                PREPARE d2 FROM @sql; EXECUTE d2; DEALLOCATE PREPARE d2;
            ");

            migrationBuilder.Sql(@"
                SET @unit_col_exists = (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Intercoms' AND COLUMN_NAME = 'UnitId'
                );
                SET @sql := IF(@unit_col_exists > 0,
                    'ALTER TABLE `Intercoms` DROP COLUMN `UnitId`;',
                    'SELECT 1');
                PREPARE d3 FROM @sql; EXECUTE d3; DEALLOCATE PREPARE d3;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1) Re-add UnitId column
            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "Intercoms",
                type: "int",
                nullable: true);

            // 2) Recreate index and FK for UnitId
            migrationBuilder.CreateIndex(
                name: "IX_Intercoms_UnitId",
                table: "Intercoms",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Intercoms_Units_UnitId",
                table: "Intercoms",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "UnitId",
                onDelete: ReferentialAction.SetNull);

            // 3) Drop FKs and indexes for Building/Amenity
            migrationBuilder.DropForeignKey(
                name: "FK_Intercoms_Buildings_BuildingId",
                table: "Intercoms");

            migrationBuilder.DropForeignKey(
                name: "FK_Intercoms_Amenities_AmenityId",
                table: "Intercoms");

            migrationBuilder.DropIndex(
                name: "IX_Intercoms_BuildingId",
                table: "Intercoms");

            migrationBuilder.DropIndex(
                name: "IX_Intercoms_AmenityId",
                table: "Intercoms");

            // 4) Drop new columns
            migrationBuilder.DropColumn(
                name: "BuildingId",
                table: "Intercoms");

            migrationBuilder.DropColumn(
                name: "AmenityId",
                table: "Intercoms");

            // 5) Make InstalledLocation nullable again
            migrationBuilder.AlterColumn<string>(
                name: "InstalledLocation",
                table: "Intercoms",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200,
                oldNullable: false);
        }
    }
}