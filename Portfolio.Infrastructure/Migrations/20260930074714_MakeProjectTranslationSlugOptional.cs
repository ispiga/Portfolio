using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeProjectTranslationSlugOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectTranslations_LanguageCode_Slug",
                table: "ProjectTranslations");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "ProjectTranslations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTranslations_LanguageCode_Slug",
                table: "ProjectTranslations",
                columns: new[] { "LanguageCode", "Slug" },
                unique: true,
                filter: "[Slug] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectTranslations_LanguageCode_Slug",
                table: "ProjectTranslations");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "ProjectTranslations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTranslations_LanguageCode_Slug",
                table: "ProjectTranslations",
                columns: new[] { "LanguageCode", "Slug" },
                unique: true);
        }
    }
}
