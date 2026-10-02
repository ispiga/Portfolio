using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogPostAdministration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EditorialStatus",
                table: "BlogPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE [BlogPosts] SET [EditorialStatus] = CASE WHEN [IsPublished] = 1 THEN 2 ELSE 0 END");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "BlogPosts");

            migrationBuilder.CreateTable(
                name: "BlogPostImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlogPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPostImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogPostImages_BlogPosts_BlogPostId",
                        column: x => x.BlogPostId,
                        principalTable: "BlogPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_BlogPosts_EditorialStatus",
                table: "BlogPosts",
                sql: "[EditorialStatus] IN (0, 1, 2)");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostImages_BlogPostId_CreatedAt",
                table: "BlogPostImages",
                columns: new[] { "BlogPostId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogPostImages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BlogPosts_EditorialStatus",
                table: "BlogPosts");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "BlogPosts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE [BlogPosts] SET [IsPublished] = CASE WHEN [EditorialStatus] = 2 THEN 1 ELSE 0 END");

            migrationBuilder.DropColumn(
                name: "EditorialStatus",
                table: "BlogPosts");
        }
    }
}
