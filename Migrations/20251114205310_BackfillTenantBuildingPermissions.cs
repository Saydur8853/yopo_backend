using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class BackfillTenantBuildingPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill UserBuildingPermissions for existing tenant users based on unit allocations.
            // For every unit that has a TenantId, ensure there is a UserBuildingPermission
            // linking that tenant user to the unit's building.
            migrationBuilder.Sql(@"
                INSERT INTO `UserBuildingPermissions` (`UserId`, `BuildingId`)
                SELECT DISTINCT u.`TenantId`, u.`BuildingId`
                FROM `Units` u
                LEFT JOIN `UserBuildingPermissions` ubp
                    ON ubp.`UserId` = u.`TenantId` AND ubp.`BuildingId` = u.`BuildingId`
                WHERE u.`TenantId` IS NOT NULL
                  AND ubp.`Id` IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: we don't remove backfilled permissions on downgrade to avoid data loss.
        }
    }
}
