using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificationAttachmentVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "CertificationAttachments",
                type: "bit",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE [CertificationAttachments] SET [IsPublic] = 1 WHERE [IsPublic] IS NULL;");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPublic",
                table: "CertificationAttachments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "CertificationAttachments");
        }
    }
}
