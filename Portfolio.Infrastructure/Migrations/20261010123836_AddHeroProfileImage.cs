using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroProfileImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF COL_LENGTH(N'dbo.HeroContent', N'ProfileImageAlternativeText') IS NULL ALTER TABLE [dbo].[HeroContent] ADD [ProfileImageAlternativeText] nvarchar(200) NULL;");
            migrationBuilder.Sql("IF COL_LENGTH(N'dbo.HeroContent', N'ProfileImageContentType') IS NULL ALTER TABLE [dbo].[HeroContent] ADD [ProfileImageContentType] nvarchar(100) NULL;");
            migrationBuilder.Sql("IF COL_LENGTH(N'dbo.HeroContent', N'ProfileImageId') IS NULL ALTER TABLE [dbo].[HeroContent] ADD [ProfileImageId] uniqueidentifier NULL;");
            migrationBuilder.Sql("IF COL_LENGTH(N'dbo.HeroContent', N'ProfileImageSizeBytes') IS NULL ALTER TABLE [dbo].[HeroContent] ADD [ProfileImageSizeBytes] bigint NULL;");
            migrationBuilder.Sql("IF COL_LENGTH(N'dbo.HeroContent', N'ProfileImageStorageKey') IS NULL ALTER TABLE [dbo].[HeroContent] ADD [ProfileImageStorageKey] nvarchar(255) NULL;");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_HeroContent_ProfileImage' AND parent_object_id = OBJECT_ID(N'dbo.HeroContent')) ALTER TABLE [dbo].[HeroContent] ADD CONSTRAINT [CK_HeroContent_ProfileImage] CHECK (([ProfileImageId] IS NULL AND [ProfileImageStorageKey] IS NULL AND [ProfileImageContentType] IS NULL AND [ProfileImageSizeBytes] IS NULL AND [ProfileImageAlternativeText] IS NULL) OR ([ProfileImageId] IS NOT NULL AND [ProfileImageStorageKey] IS NOT NULL AND [ProfileImageContentType] IS NOT NULL AND [ProfileImageSizeBytes] > 0 AND [ProfileImageAlternativeText] IS NOT NULL));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_HeroContent_ProfileImage' AND parent_object_id = OBJECT_ID(N'dbo.HeroContent')) ALTER TABLE [dbo].[HeroContent] DROP CONSTRAINT [CK_HeroContent_ProfileImage];");
            migrationBuilder.Sql("IF COL_LENGTH(N'dbo.HeroContent', N'ProfileImageAlternativeText') IS NOT NULL ALTER TABLE [dbo].[HeroContent] DROP COLUMN [ProfileImageAlternativeText];");
            migrationBuilder.Sql("IF COL_LENGTH(N'dbo.HeroContent', N'ProfileImageContentType') IS NOT NULL ALTER TABLE [dbo].[HeroContent] DROP COLUMN [ProfileImageContentType];");
            migrationBuilder.Sql("IF COL_LENGTH(N'dbo.HeroContent', N'ProfileImageId') IS NOT NULL ALTER TABLE [dbo].[HeroContent] DROP COLUMN [ProfileImageId];");
            migrationBuilder.Sql("IF COL_LENGTH(N'dbo.HeroContent', N'ProfileImageSizeBytes') IS NOT NULL ALTER TABLE [dbo].[HeroContent] DROP COLUMN [ProfileImageSizeBytes];");
            migrationBuilder.Sql("IF COL_LENGTH(N'dbo.HeroContent', N'ProfileImageStorageKey') IS NOT NULL ALTER TABLE [dbo].[HeroContent] DROP COLUMN [ProfileImageStorageKey];");
        }
    }
}
