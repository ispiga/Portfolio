using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVisualIdentitySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VisualIdentitySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    PrimaryColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    SecondaryColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    AccentColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    LogoColorsLinkedToPalette = table.Column<bool>(type: "bit", nullable: false),
                    LogoColorStartOverride = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    LogoColorEndOverride = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    MainLogoStorageKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MainLogoSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    MainLogoSupportsRecoloring = table.Column<bool>(type: "bit", nullable: false),
                    FooterLogoStorageKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FooterLogoSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    FaviconStorageKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FaviconSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    FooterLogoNeedsReview = table.Column<bool>(type: "bit", nullable: false),
                    FaviconNeedsReview = table.Column<bool>(type: "bit", nullable: false),
                    AssetVersion = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisualIdentitySettings", x => x.Id);
                    table.CheckConstraint("CK_VisualIdentitySettings_AssetVersion", "[AssetVersion] <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_VisualIdentitySettings_Colors", "[PrimaryColor] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]' AND [SecondaryColor] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]' AND [AccentColor] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'");
                    table.CheckConstraint("CK_VisualIdentitySettings_LogoOverrides", "([LogoColorStartOverride] IS NULL AND [LogoColorEndOverride] IS NULL) OR ([LogoColorsLinkedToPalette] = 0 AND [LogoColorStartOverride] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]' AND [LogoColorEndOverride] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]')");
                    table.CheckConstraint("CK_VisualIdentitySettings_MainLogo", "([MainLogoStorageKey] IS NULL AND [MainLogoSizeBytes] IS NULL) OR ([MainLogoStorageKey] IS NOT NULL AND [MainLogoSizeBytes] > 0)");
                    table.CheckConstraint("CK_VisualIdentitySettings_Singleton", "[Id] = 1");
                });

            migrationBuilder.InsertData(
                table: "VisualIdentitySettings",
                columns: new[] { "Id", "AccentColor", "AssetVersion", "FaviconNeedsReview", "FaviconSizeBytes", "FaviconStorageKey", "FooterLogoNeedsReview", "FooterLogoSizeBytes", "FooterLogoStorageKey", "LogoColorEndOverride", "LogoColorStartOverride", "LogoColorsLinkedToPalette", "MainLogoSizeBytes", "MainLogoStorageKey", "MainLogoSupportsRecoloring", "PrimaryColor", "SecondaryColor" },
                values: new object[] { 1, "#C98F42", new Guid("00000000-0000-0000-0000-000000000001"), false, null, null, false, null, null, null, null, true, null, null, true, "#00A3FF", "#3E00D1" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VisualIdentitySettings");
        }
    }
}
