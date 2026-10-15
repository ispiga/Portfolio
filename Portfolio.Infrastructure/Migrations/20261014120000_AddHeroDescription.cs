using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Portfolio.Infrastructure;

#nullable disable

namespace Portfolio.Infrastructure.Migrations;

[DbContext(typeof(PortfolioDbContext))]
[Migration("20261014120000_AddHeroDescription")]
public partial class AddHeroDescription : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "HeroTranslations",
            type: "nvarchar(2000)",
            maxLength: 2000,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Description", table: "HeroTranslations");
    }
}
