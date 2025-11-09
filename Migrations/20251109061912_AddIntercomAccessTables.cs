using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddIntercomAccessTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS `IntercomMasterPins` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `IntercomId` INT NOT NULL,
  `PinHash` VARCHAR(255) NOT NULL,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `CreatedBy` INT NOT NULL,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedBy` INT NULL,
  `UpdatedAt` DATETIME NULL,
  PRIMARY KEY (`Id`),
  INDEX `IX_IntercomMasterPins_IntercomId_IsActive` (`IntercomId`, `IsActive`),
  CONSTRAINT `FK_IntercomMasterPins_Intercoms_IntercomId` FOREIGN KEY (`IntercomId`) REFERENCES `Intercoms` (`IntercomId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS `IntercomUserPins` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `IntercomId` INT NOT NULL,
  `UserId` INT NOT NULL,
  `PinHash` VARCHAR(255) NOT NULL,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `CreatedBy` INT NOT NULL,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedBy` INT NULL,
  `UpdatedAt` DATETIME NULL,
  PRIMARY KEY (`Id`),
  UNIQUE INDEX `UX_IntercomUserPins_IntercomId_UserId` (`IntercomId`, `UserId`),
  CONSTRAINT `FK_IntercomUserPins_Intercoms_IntercomId` FOREIGN KEY (`IntercomId`) REFERENCES `Intercoms` (`IntercomId`) ON DELETE CASCADE,
  CONSTRAINT `FK_IntercomUserPins_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS `IntercomTemporaryPins` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `IntercomId` INT NOT NULL,
  `CreatedByUserId` INT NOT NULL,
  `PinHash` VARCHAR(255) NOT NULL,
  `ExpiresAt` DATETIME NOT NULL,
  `MaxUses` INT NOT NULL,
  `UsesCount` INT NOT NULL DEFAULT 0,
  `FirstUsedAt` DATETIME NULL,
  `LastUsedAt` DATETIME NULL,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  INDEX `IX_IntercomTemporaryPins_IntercomId_IsActive` (`IntercomId`, `IsActive`),
  INDEX `IX_IntercomTemporaryPins_CreatedByUserId` (`CreatedByUserId`),
  CONSTRAINT `FK_IntercomTemporaryPins_Intercoms_IntercomId` FOREIGN KEY (`IntercomId`) REFERENCES `Intercoms` (`IntercomId`) ON DELETE CASCADE,
  CONSTRAINT `FK_IntercomTemporaryPins_Users_CreatedByUserId` FOREIGN KEY (`CreatedByUserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS `IntercomTemporaryPinUsages` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `TemporaryPinId` INT NOT NULL,
  `UsedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UsedFromIp` VARCHAR(50) NULL,
  `DeviceInfo` VARCHAR(200) NULL,
  PRIMARY KEY (`Id`),
  INDEX `IX_IntercomTemporaryPinUsages_TemporaryPinId` (`TemporaryPinId`),
  CONSTRAINT `FK_IntercomTemporaryPinUsages_TemporaryPins_TemporaryPinId` FOREIGN KEY (`TemporaryPinId`) REFERENCES `IntercomTemporaryPins` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS `IntercomAccessLogs` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `IntercomId` INT NOT NULL,
  `UserId` INT NULL,
  `CredentialType` VARCHAR(20) NOT NULL,
  `CredentialRefId` INT NULL,
  `IsSuccess` TINYINT(1) NOT NULL,
  `Reason` VARCHAR(200) NULL,
  `OccurredAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IpAddress` VARCHAR(50) NULL,
  `DeviceInfo` VARCHAR(200) NULL,
  PRIMARY KEY (`Id`),
  INDEX `IX_IntercomAccessLogs_IntercomId` (`IntercomId`),
  INDEX `IX_IntercomAccessLogs_OccurredAt` (`OccurredAt`),
  CONSTRAINT `FK_IntercomAccessLogs_Intercoms_IntercomId` FOREIGN KEY (`IntercomId`) REFERENCES `Intercoms` (`IntercomId`) ON DELETE CASCADE,
  CONSTRAINT `FK_IntercomAccessLogs_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS `IntercomAccessLogs`;");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS `IntercomTemporaryPinUsages`;");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS `IntercomTemporaryPins`;");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS `IntercomUserPins`;");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS `IntercomMasterPins`;");
        }
    }
}
