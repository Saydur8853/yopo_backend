using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using YopoBackend.Data;

#nullable disable

namespace YopoBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20251104003000_IntercomAuditAndConstraints")]
    public partial class IntercomAuditAndConstraints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add CreatedBy and UpdatedBy (nullable) if not present
            migrationBuilder.Sql(@"
                SET @col_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Intercoms' AND COLUMN_NAME = 'CreatedBy'
                );
                SET @sql := IF(@col_exists = 0,
                    'ALTER TABLE `Intercoms` ADD COLUMN `CreatedBy` int NULL;',
                    'SELECT 1');
                PREPARE s1 FROM @sql; EXECUTE s1; DEALLOCATE PREPARE s1;
            ");

            migrationBuilder.Sql(@"
                SET @col_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Intercoms' AND COLUMN_NAME = 'UpdatedBy'
                );
                SET @sql := IF(@col_exists = 0,
                    'ALTER TABLE `Intercoms` ADD COLUMN `UpdatedBy` int NULL;',
                    'SELECT 1');
                PREPARE s2 FROM @sql; EXECUTE s2; DEALLOCATE PREPARE s2;
            ");

            // Indexes for audit columns
            migrationBuilder.Sql(@"
                SET @idx_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Intercoms' AND INDEX_NAME = 'IX_Intercoms_CreatedBy'
                );
                SET @sql := IF(@idx_exists = 0,
                    'CREATE INDEX `IX_Intercoms_CreatedBy` ON `Intercoms` (`CreatedBy`);',
                    'SELECT 1');
                PREPARE i1 FROM @sql; EXECUTE i1; DEALLOCATE PREPARE i1;
            ");

            migrationBuilder.Sql(@"
                SET @idx_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Intercoms' AND INDEX_NAME = 'IX_Intercoms_UpdatedBy'
                );
                SET @sql := IF(@idx_exists = 0,
                    'CREATE INDEX `IX_Intercoms_UpdatedBy` ON `Intercoms` (`UpdatedBy`);',
                    'SELECT 1');
                PREPARE i2 FROM @sql; EXECUTE i2; DEALLOCATE PREPARE i2;
            ");

            // FKs to Users for audit columns (Restrict delete)
            migrationBuilder.Sql(@"
                SET @fk_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
                    WHERE CONSTRAINT_SCHEMA = DATABASE() AND CONSTRAINT_NAME = 'FK_Intercoms_Users_CreatedBy'
                );
                SET @sql := IF(@fk_exists = 0,
                    'ALTER TABLE `Intercoms` ADD CONSTRAINT `FK_Intercoms_Users_CreatedBy` FOREIGN KEY (`CreatedBy`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT;',
                    'SELECT 1');
                PREPARE f1 FROM @sql; EXECUTE f1; DEALLOCATE PREPARE f1;
            ");

            migrationBuilder.Sql(@"
                SET @fk_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
                    WHERE CONSTRAINT_SCHEMA = DATABASE() AND CONSTRAINT_NAME = 'FK_Intercoms_Users_UpdatedBy'
                );
                SET @sql := IF(@fk_exists = 0,
                    'ALTER TABLE `Intercoms` ADD CONSTRAINT `FK_Intercoms_Users_UpdatedBy` FOREIGN KEY (`UpdatedBy`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT;',
                    'SELECT 1');
                PREPARE f2 FROM @sql; EXECUTE f2; DEALLOCATE PREPARE f2;
            ");

            // Make BuildingId NOT NULL if possible (after ensuring no NULLs remain)
            migrationBuilder.Sql(@"
                SET @nulls := (SELECT COUNT(*) FROM Intercoms WHERE BuildingId IS NULL);
                SET @sql := IF(@nulls = 0,
                    'ALTER TABLE `Intercoms` MODIFY COLUMN `BuildingId` int NOT NULL;',
                    'SELECT 1');
                PREPARE nb FROM @sql; EXECUTE nb; DEALLOCATE PREPARE nb;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FKs
            migrationBuilder.Sql(@"
                SET @fk_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
                    WHERE CONSTRAINT_SCHEMA = DATABASE() AND CONSTRAINT_NAME = 'FK_Intercoms_Users_CreatedBy'
                );
                SET @sql := IF(@fk_exists > 0,
                    'ALTER TABLE `Intercoms` DROP FOREIGN KEY `FK_Intercoms_Users_CreatedBy`;',
                    'SELECT 1');
                PREPARE df1 FROM @sql; EXECUTE df1; DEALLOCATE PREPARE df1;
            ");

            migrationBuilder.Sql(@"
                SET @fk_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
                    WHERE CONSTRAINT_SCHEMA = DATABASE() AND CONSTRAINT_NAME = 'FK_Intercoms_Users_UpdatedBy'
                );
                SET @sql := IF(@fk_exists > 0,
                    'ALTER TABLE `Intercoms` DROP FOREIGN KEY `FK_Intercoms_Users_UpdatedBy`;',
                    'SELECT 1');
                PREPARE df2 FROM @sql; EXECUTE df2; DEALLOCATE PREPARE df2;
            ");

            // Drop indexes
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS `IX_Intercoms_CreatedBy` ON `Intercoms`;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS `IX_Intercoms_UpdatedBy` ON `Intercoms`;");

            // Drop columns
            migrationBuilder.Sql(@"
                SET @col_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Intercoms' AND COLUMN_NAME = 'CreatedBy'
                );
                SET @sql := IF(@col_exists > 0,
                    'ALTER TABLE `Intercoms` DROP COLUMN `CreatedBy`;',
                    'SELECT 1');
                PREPARE dc1 FROM @sql; EXECUTE dc1; DEALLOCATE PREPARE dc1;
            ");

            migrationBuilder.Sql(@"
                SET @col_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Intercoms' AND COLUMN_NAME = 'UpdatedBy'
                );
                SET @sql := IF(@col_exists > 0,
                    'ALTER TABLE `Intercoms` DROP COLUMN `UpdatedBy`;',
                    'SELECT 1');
                PREPARE dc2 FROM @sql; EXECUTE dc2; DEALLOCATE PREPARE dc2;
            ");
        }
    }
}
