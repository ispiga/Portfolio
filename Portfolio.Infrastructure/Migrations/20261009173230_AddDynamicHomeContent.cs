using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicHomeContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AboutProfile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutProfile", x => x.Id);
                    table.CheckConstraint("CK_AboutProfile_Singleton", "[Id] = 1");
                });

            migrationBuilder.CreateTable(
                name: "HeroContent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    OrbitCount = table.Column<int>(type: "int", nullable: false),
                    ProfileImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProfileImageStorageKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ProfileImageContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProfileImageSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    ProfileImageAlternativeText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeroContent", x => x.Id);
                    table.CheckConstraint("CK_HeroContent_OrbitCount", "[OrbitCount] >= 1");
                    table.CheckConstraint("CK_HeroContent_ProfileImage", "([ProfileImageId] IS NULL AND [ProfileImageStorageKey] IS NULL AND [ProfileImageContentType] IS NULL AND [ProfileImageSizeBytes] IS NULL AND [ProfileImageAlternativeText] IS NULL) OR ([ProfileImageId] IS NOT NULL AND [ProfileImageStorageKey] IS NOT NULL AND [ProfileImageContentType] IS NOT NULL AND [ProfileImageSizeBytes] > 0 AND [ProfileImageAlternativeText] IS NOT NULL)");
                    table.CheckConstraint("CK_HeroContent_Singleton", "[Id] = 1");
                });

            migrationBuilder.CreateTable(
                name: "Hobbies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hobbies", x => x.Id);
                    table.CheckConstraint("CK_Hobbies_DisplayOrder", "[DisplayOrder] >= 0");
                    table.CheckConstraint("CK_Hobbies_SizeBytes", "[SizeBytes] > 0");
                });

            migrationBuilder.CreateTable(
                name: "SkillGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillGroups", x => x.Id);
                    table.CheckConstraint("CK_SkillGroups_DisplayOrder", "[DisplayOrder] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "TechnologyLogos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AlternativeText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Orbit = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnologyLogos", x => x.Id);
                    table.CheckConstraint("CK_TechnologyLogos_DisplayOrder", "[DisplayOrder] >= 0");
                    table.CheckConstraint("CK_TechnologyLogos_Orbit", "[Orbit] >= 1");
                    table.CheckConstraint("CK_TechnologyLogos_SizeBytes", "[SizeBytes] > 0");
                });

            migrationBuilder.CreateTable(
                name: "AboutProfileTranslations",
                columns: table => new
                {
                    AboutProfileId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutProfileTranslations", x => new { x.AboutProfileId, x.LanguageCode });
                    table.CheckConstraint("CK_AboutProfileTranslations_LanguageCode", "[LanguageCode] IN ('es-ES', 'en-US')");
                    table.ForeignKey(
                        name: "FK_AboutProfileTranslations_AboutProfile_AboutProfileId",
                        column: x => x.AboutProfileId,
                        principalTable: "AboutProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HeroTranslations",
                columns: table => new
                {
                    HeroContentId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Headline = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeroTranslations", x => new { x.HeroContentId, x.LanguageCode });
                    table.CheckConstraint("CK_HeroTranslations_LanguageCode", "[LanguageCode] IN ('es-ES', 'en-US')");
                    table.ForeignKey(
                        name: "FK_HeroTranslations_HeroContent_HeroContentId",
                        column: x => x.HeroContentId,
                        principalTable: "HeroContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HobbyTranslations",
                columns: table => new
                {
                    HobbyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1200)", maxLength: 1200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HobbyTranslations", x => new { x.HobbyId, x.LanguageCode });
                    table.CheckConstraint("CK_HobbyTranslations_LanguageCode", "[LanguageCode] IN ('es-ES', 'en-US')");
                    table.ForeignKey(
                        name: "FK_HobbyTranslations_Hobbies_HobbyId",
                        column: x => x.HobbyId,
                        principalTable: "Hobbies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkillGroupTranslations",
                columns: table => new
                {
                    SkillGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillGroupTranslations", x => new { x.SkillGroupId, x.LanguageCode });
                    table.CheckConstraint("CK_SkillGroupTranslations_LanguageCode", "[LanguageCode] IN ('es-ES', 'en-US')");
                    table.ForeignKey(
                        name: "FK_SkillGroupTranslations_SkillGroups_SkillGroupId",
                        column: x => x.SkillGroupId,
                        principalTable: "SkillGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                    table.CheckConstraint("CK_Skills_DisplayOrder", "[DisplayOrder] >= 0");
                    table.ForeignKey(
                        name: "FK_Skills_SkillGroups_SkillGroupId",
                        column: x => x.SkillGroupId,
                        principalTable: "SkillGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkillTranslations",
                columns: table => new
                {
                    SkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillTranslations", x => new { x.SkillId, x.LanguageCode });
                    table.CheckConstraint("CK_SkillTranslations_LanguageCode", "[LanguageCode] IN ('es-ES', 'en-US')");
                    table.ForeignKey(
                        name: "FK_SkillTranslations_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hobbies_DisplayOrder",
                table: "Hobbies",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_SkillGroups_DisplayOrder",
                table: "SkillGroups",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_SkillGroupId_DisplayOrder",
                table: "Skills",
                columns: new[] { "SkillGroupId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyLogos_Orbit_DisplayOrder",
                table: "TechnologyLogos",
                columns: new[] { "Orbit", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AboutProfileTranslations");

            migrationBuilder.DropTable(
                name: "HeroTranslations");

            migrationBuilder.DropTable(
                name: "HobbyTranslations");

            migrationBuilder.DropTable(
                name: "SkillGroupTranslations");

            migrationBuilder.DropTable(
                name: "SkillTranslations");

            migrationBuilder.DropTable(
                name: "TechnologyLogos");

            migrationBuilder.DropTable(
                name: "AboutProfile");

            migrationBuilder.DropTable(
                name: "HeroContent");

            migrationBuilder.DropTable(
                name: "Hobbies");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "SkillGroups");
        }
    }
}
