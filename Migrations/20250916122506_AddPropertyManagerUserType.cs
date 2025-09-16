using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyManagerUserType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert Property Manager user type with OWN data access control
            migrationBuilder.Sql(@"
                INSERT INTO UserTypes (Id, Name, Description, DataAccessControl, IsActive, CreatedBy, CreatedAt)
                VALUES (2, 'Property Manager', 'Property manager with access to all modules but limited to managing only their own created data', 'OWN', 1, 1, UTC_TIMESTAMP())
                ON DUPLICATE KEY UPDATE 
                    Name = VALUES(Name),
                    Description = VALUES(Description),
                    DataAccessControl = 'OWN',
                    UpdatedAt = UTC_TIMESTAMP();
            ");

            // Grant Property Manager access to all active modules
            migrationBuilder.Sql(@"
                INSERT INTO UserTypeModulePermissions (UserTypeId, ModuleId, IsActive, CreatedAt)
                SELECT 2, m.Id, 1, UTC_TIMESTAMP()
                FROM Modules m 
                WHERE m.IsActive = 1
                AND NOT EXISTS (
                    SELECT 1 FROM UserTypeModulePermissions utmp 
                    WHERE utmp.UserTypeId = 2 AND utmp.ModuleId = m.Id
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove Property Manager module permissions
            migrationBuilder.Sql("DELETE FROM UserTypeModulePermissions WHERE UserTypeId = 2;");
            
            // Remove Property Manager user type
            migrationBuilder.Sql("DELETE FROM UserTypes WHERE Id = 2;");
        }
    }
}
