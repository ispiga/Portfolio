using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificationTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CertificationTranslations",
                columns: table => new
                {
                    CertificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Issuer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificationTranslations", x => new { x.CertificationId, x.LanguageCode });
                    table.CheckConstraint("CK_CertificationTranslations_LanguageCode", "[LanguageCode] IN ('es-ES', 'en-US')");
                    table.ForeignKey(
                        name: "FK_CertificationTranslations_Certifications_CertificationId",
                        column: x => x.CertificationId,
                        principalTable: "Certifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO [CertificationTranslations] ([CertificationId], [LanguageCode], [Name], [Issuer])
                SELECT [Id], 'es-ES', [Name], [Issuer]
                FROM [Certifications];
                """);

            migrationBuilder.DropColumn(
                name: "Issuer",
                table: "Certifications");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Certifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificationTranslations");

            migrationBuilder.AddColumn<string>(
                name: "Issuer",
                table: "Certifications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Certifications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
