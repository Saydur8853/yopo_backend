using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddDataAccessControlToUserType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataAccessControl",
                table: "UserTypes",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "ALL")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Buildings",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataAccessControl",
                table: "UserTypes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Buildings");
        }
    }
}
