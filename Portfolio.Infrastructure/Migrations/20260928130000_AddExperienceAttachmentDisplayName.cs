using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Portfolio.Infrastructure;

#nullable disable

namespace Portfolio.Infrastructure.Migrations;

[DbContext(typeof(PortfolioDbContext))]
[Migration("20260928130000_AddExperienceAttachmentDisplayName")]
public sealed class AddExperienceAttachmentDisplayName : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "DisplayName",
            table: "ExperienceAttachments",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: true);

        migrationBuilder.Sql("UPDATE [ExperienceAttachments] SET [DisplayName] = [OriginalFileName]");

        migrationBuilder.AlterColumn<string>(
            name: "DisplayName",
            table: "ExperienceAttachments",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldMaxLength: 255,
            oldNullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DisplayName",
            table: "ExperienceAttachments");
    }
}
