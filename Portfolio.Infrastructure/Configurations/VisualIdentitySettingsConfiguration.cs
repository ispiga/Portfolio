using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public sealed class VisualIdentitySettingsConfiguration : IEntityTypeConfiguration<VisualIdentitySettings>
{
    public void Configure(EntityTypeBuilder<VisualIdentitySettings> builder)
    {
        builder.ToTable("VisualIdentitySettings", table =>
        {
            table.HasCheckConstraint("CK_VisualIdentitySettings_Singleton", "[Id] = 1");
            table.HasCheckConstraint("CK_VisualIdentitySettings_Colors", "[PrimaryColor] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]' AND [SecondaryColor] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]' AND [AccentColor] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'");
            table.HasCheckConstraint("CK_VisualIdentitySettings_LogoOverrides", "([LogoColorStartOverride] IS NULL AND [LogoColorEndOverride] IS NULL) OR ([LogoColorsLinkedToPalette] = 0 AND [LogoColorStartOverride] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]' AND [LogoColorEndOverride] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]')");
            table.HasCheckConstraint("CK_VisualIdentitySettings_MainLogo", "([MainLogoStorageKey] IS NULL AND [MainLogoSizeBytes] IS NULL) OR ([MainLogoStorageKey] IS NOT NULL AND [MainLogoSizeBytes] > 0)");
            table.HasCheckConstraint("CK_VisualIdentitySettings_AssetVersion", "[AssetVersion] <> '00000000-0000-0000-0000-000000000000'");
        });

        builder.HasKey(settings => settings.Id);
        builder.Property(settings => settings.Id).ValueGeneratedNever();
        builder.Property(settings => settings.PrimaryColor).HasMaxLength(7).IsRequired();
        builder.Property(settings => settings.SecondaryColor).HasMaxLength(7).IsRequired();
        builder.Property(settings => settings.AccentColor).HasMaxLength(7).IsRequired();
        builder.Property(settings => settings.LogoColorStartOverride).HasMaxLength(7);
        builder.Property(settings => settings.LogoColorEndOverride).HasMaxLength(7);
        builder.Property(settings => settings.MainLogoStorageKey).HasMaxLength(255);
        builder.Property(settings => settings.FooterLogoStorageKey).HasMaxLength(255);
        builder.Property(settings => settings.FaviconStorageKey).HasMaxLength(255);
        builder.Property(settings => settings.AssetVersion).IsRequired();

        builder.HasData(new VisualIdentitySettings
        {
            Id = 1,
            PrimaryColor = "#00A3FF",
            SecondaryColor = "#3E00D1",
            AccentColor = "#C98F42",
            LogoColorsLinkedToPalette = true,
            MainLogoSupportsRecoloring = true,
            AssetVersion = Guid.Parse("00000000-0000-0000-0000-000000000001")
        });
    }
}
