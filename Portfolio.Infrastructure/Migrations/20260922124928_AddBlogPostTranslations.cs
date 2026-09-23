using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogPostTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogPostTranslations",
                columns: table => new
                {
                    BlogPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Excerpt = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPostTranslations", x => new { x.BlogPostId, x.LanguageCode });
                    table.CheckConstraint("CK_BlogPostTranslations_LanguageCode", "[LanguageCode] IN ('es-ES', 'en-US')");
                    table.ForeignKey(
                        name: "FK_BlogPostTranslations_BlogPosts_BlogPostId",
                        column: x => x.BlogPostId,
                        principalTable: "BlogPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostTranslations_LanguageCode_Slug",
                table: "BlogPostTranslations",
                columns: new[] { "LanguageCode", "Slug" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO [BlogPostTranslations] ([BlogPostId], [LanguageCode], [Title], [Slug], [Excerpt], [Content])
                SELECT [Id], N'es-ES', [Title], [Slug], [Excerpt], [Content]
                FROM [BlogPosts];
                """);

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_Slug",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "Excerpt",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "BlogPosts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "BlogPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Excerpt",
                table: "BlogPosts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "BlogPosts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "BlogPosts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE posts
                SET [Title] = translations.[Title],
                    [Slug] = translations.[Slug],
                    [Excerpt] = translations.[Excerpt],
                    [Content] = translations.[Content]
                FROM [BlogPosts] posts
                INNER JOIN [BlogPostTranslations] translations
                    ON translations.[BlogPostId] = posts.[Id]
                    AND translations.[LanguageCode] = N'es-ES';
                """);

            migrationBuilder.DropTable(
                name: "BlogPostTranslations");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_Slug",
                table: "BlogPosts",
                column: "Slug",
                unique: true);
        }
    }
}
