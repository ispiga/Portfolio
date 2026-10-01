using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificationMetadataAndAttachmentDisplayName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CredentialId",
                table: "Certifications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Hours",
                table: "Certifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "CertificationAttachments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE [CertificationAttachments] SET [DisplayName] = [OriginalFileName] WHERE [DisplayName] = N'';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CredentialId",
                table: "Certifications");

            migrationBuilder.DropColumn(
                name: "Hours",
                table: "Certifications");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "CertificationAttachments");
        }
    }
}
