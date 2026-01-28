using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YopoBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFaceBiometricToCloudinary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RightImagePath",
                table: "IntercomFaceBiometrics",
                newName: "RightImagePublicId");

            migrationBuilder.RenameColumn(
                name: "LeftImagePath",
                table: "IntercomFaceBiometrics",
                newName: "LeftImagePublicId");

            migrationBuilder.RenameColumn(
                name: "FrontImagePath",
                table: "IntercomFaceBiometrics",
                newName: "FrontImagePublicId");

            migrationBuilder.AddColumn<string>(
                name: "FrontImageUrl",
                table: "IntercomFaceBiometrics",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LeftImageUrl",
                table: "IntercomFaceBiometrics",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RightImageUrl",
                table: "IntercomFaceBiometrics",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FrontImageUrl",
                table: "IntercomFaceBiometrics");

            migrationBuilder.DropColumn(
                name: "LeftImageUrl",
                table: "IntercomFaceBiometrics");

            migrationBuilder.DropColumn(
                name: "RightImageUrl",
                table: "IntercomFaceBiometrics");

            migrationBuilder.RenameColumn(
                name: "RightImagePublicId",
                table: "IntercomFaceBiometrics",
                newName: "RightImagePath");

            migrationBuilder.RenameColumn(
                name: "LeftImagePublicId",
                table: "IntercomFaceBiometrics",
                newName: "LeftImagePath");

            migrationBuilder.RenameColumn(
                name: "FrontImagePublicId",
                table: "IntercomFaceBiometrics",
                newName: "FrontImagePath");
        }
    }
}
