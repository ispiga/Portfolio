using System.Globalization;
using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Portfolio.Application.HomeContent;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Identity;
using Portfolio.Infrastructure.Services;
using Portfolio.Infrastructure.Storage;
using Portfolio.Web.Components.Admin;
using Xunit;

namespace Portfolio.Tests;

public sealed class HomeContentTests
{
    [Fact]
    public async Task Hero_admin_and_public_query_preserve_localization_orbit_and_order()
    {
        var factory = CreateFactory();
        var storage = new FakeHomeContentImageStorageService();
        var admin = new HeroContentAdministrationService(factory, storage);
        var inner = Logo(0, 1, "C#");
        var outer = Logo(1, 2, ".NET");
        var request = new HeroEditRequest(2, "Titular español", "English headline", [outer, inner]);

        Assert.True((await admin.SaveAsync(request)).Succeeded);

        var previousCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var result = await new HeroQueryService(factory).GetAsync();
            Assert.NotNull(result);
            Assert.Equal("English headline", result.Headline);
            Assert.Equal(2, result.OrbitCount);
            Assert.Equal(new[] { "C#", ".NET" }, result.Logos.Select(logo => logo.Name));
            Assert.Equal(new[] { 1, 2 }, result.Logos.Select(logo => logo.Orbit));
            Assert.All(result.Logos, logo => Assert.Contains("/home-content-images/technology-logo/", logo.ImageUrl, StringComparison.Ordinal));

            Assert.True((await admin.SaveAsync(request with { EnglishHeadline = null })).Succeeded);
            result = await new HeroQueryService(factory).GetAsync();
            Assert.NotNull(result);
            Assert.Equal("Titular español", result.Headline);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    [Fact]
    public async Task Hero_profile_image_is_saved_read_and_removed_with_its_file()
    {
        var factory = CreateFactory();
        var storage = new FakeHomeContentImageStorageService();
        var admin = new HeroContentAdministrationService(factory, storage);
        var imageId = Guid.NewGuid();
        var storageKey = $"HeroProfiles/{imageId:N}/{Guid.NewGuid():N}.png";
        var request = new HeroEditRequest(
            3, "Titular", null, [], imageId, storageKey, "image/png", 8, "Retrato personal");

        Assert.True((await admin.SaveAsync(request)).Succeeded);
        var saved = await admin.GetAsync();
        Assert.Equal(imageId, saved.ProfileImageId);
        Assert.Equal(storageKey, saved.ProfileImageStorageKey);
        Assert.Equal("Retrato personal", saved.ProfileImageAlternativeText);
        using (var context = factory.CreateDbContext())
        {
            var heroContent = Assert.Single(context.HeroContents);
            Assert.Equal(imageId, heroContent.ProfileImageId);
            Assert.Equal(storageKey, heroContent.ProfileImageStorageKey);
        }

        var publicHero = await new HeroQueryService(factory).GetAsync();
        Assert.NotNull(publicHero);
        Assert.Equal($"/home-content-images/hero-profile/{imageId:N}", publicHero.ProfileImageUrl);
        Assert.Equal("Retrato personal", publicHero.ProfileImageAlternativeText);

        Assert.True((await admin.SaveAsync(request with
        {
            ProfileImageId = null,
            ProfileImageStorageKey = null,
            ProfileImageContentType = null,
            ProfileImageSizeBytes = null,
            ProfileImageAlternativeText = null
        })).Succeeded);
        Assert.Contains(storage.Deleted, item => item.Kind == HomeContentImageKind.HeroProfile
            && item.Id == imageId && item.Key == storageKey);
    }

    [Fact]
    public async Task About_admin_and_public_query_preserve_localization_group_skill_hobby_order()
    {
        var factory = CreateFactory();
        var storage = new FakeHomeContentImageStorageService();
        var admin = new AboutAdministrationService(factory, storage);
        var firstGroup = new SkillGroupEditModel(
            Guid.NewGuid(), "Backend", null, 0,
            [new SkillEditModel(Guid.NewGuid(), "C#", null, 1), new SkillEditModel(Guid.NewGuid(), ".NET", "Dotnet", 0)]);
        var secondGroup = new SkillGroupEditModel(Guid.NewGuid(), "Datos", "Data", 1, []);
        var firstHobby = Hobby(1, "Lectura", "Novelas", null, null);
        var secondHobby = Hobby(0, "Senderismo", "Montaña", "Hiking", "Hills");
        var request = new AboutContentEditRequest(
            "Perfil español", null, [secondGroup, firstGroup], [firstHobby, secondHobby]);

        Assert.True((await admin.SaveAsync(request)).Succeeded);

        var previousCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var result = await new AboutQueryService(factory).GetAsync();
            Assert.Equal("Perfil español", result.Profile);
            Assert.Equal(new[] { "Backend", "Data" }, result.Groups.Select(group => group.Name));
            Assert.Equal(new[] { "Dotnet", "C#" }, result.Groups[0].Skills.Select(skill => skill.Name));
            Assert.Equal(new[] { "Hiking", "Lectura" }, result.Hobbies.Select(hobby => hobby.Name));
            Assert.Equal(new[] { 0, 1 }, result.Hobbies.Select(hobby => hobby.DisplayOrder));

            var changed = request with { EnglishProfile = "English profile" };
            Assert.True((await admin.SaveAsync(changed)).Succeeded);
            result = await new AboutQueryService(factory).GetAsync();
            Assert.Equal("English profile", result.Profile);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    [Fact]
    public async Task Removing_logo_and_hobby_deletes_only_their_stored_images_after_save()
    {
        var factory = CreateFactory();
        var storage = new FakeHomeContentImageStorageService();
        var heroAdmin = new HeroContentAdministrationService(factory, storage);
        var logo = Logo(0, 1, "Logo");
        Assert.True((await heroAdmin.SaveAsync(new HeroEditRequest(1, "Titular", null, [logo]))).Succeeded);
        Assert.True((await heroAdmin.SaveAsync(new HeroEditRequest(1, "Titular", null, []))).Succeeded);

        var aboutAdmin = new AboutAdministrationService(factory, storage);
        var hobby = Hobby(0, "Afición", "Descripción", null, null);
        var aboutRequest = new AboutContentEditRequest("Perfil", null, [], [hobby]);
        Assert.True((await aboutAdmin.SaveAsync(aboutRequest)).Succeeded);
        Assert.True((await aboutAdmin.SaveAsync(aboutRequest with { Hobbies = [] })).Succeeded);

        Assert.Contains(storage.Deleted, item => item.Kind == HomeContentImageKind.TechnologyLogo
            && item.Id == logo.Id && item.Key == logo.StorageKey);
        Assert.Contains(storage.Deleted, item => item.Kind == HomeContentImageKind.Hobby
            && item.Id == hobby.Id && item.Key == hobby.StorageKey);
    }

    [Fact]
    public void Validators_reject_invalid_orbit_ids_missing_media_and_partial_hobby_translation()
    {
        var invalidHero = new HeroEditRequest(
            1, string.Empty, null,
            [Logo(0, 2, "Invalid orbit") with { Id = Guid.Empty, StorageKey = string.Empty }]);
        var heroErrors = HeroContentValidator.Validate(invalidHero);
        Assert.Contains(heroErrors, error => error.ResourceKey == "HomeSpanishHeadlineRequired");
        Assert.Contains(heroErrors, error => error.ResourceKey == "HomeLogoOrbitInvalid");
        Assert.Contains(heroErrors, error => error.ResourceKey == "HomeImageRequired");

        var invalidAbout = new AboutContentEditRequest(
            string.Empty, null, [], [Hobby(0, "Afición", "Descripción", "Hobby", null)]);
        var aboutErrors = AboutContentValidator.Validate(invalidAbout);
        Assert.Contains(aboutErrors, error => error.ResourceKey == "AboutSpanishProfileRequired");
        Assert.Contains(aboutErrors, error => error.ResourceKey == "AboutEnglishHobbyIncomplete");

        var invalidImage = HeroContentValidator.Validate(new HeroEditRequest(
            1, "Titular", null, [], Guid.NewGuid(), "HeroProfiles/a.png", "image/png", 8, " "));
        Assert.Contains(invalidImage, error => error.Field == "ProfileImage");
    }

    [Fact]
    public void Hero_validator_rejects_more_than_three_orbits()
    {
        var errors = HeroContentValidator.Validate(new HeroEditRequest(
            4, "Titular", null, []));

        Assert.Contains(errors, error => error.Field == nameof(HeroEditRequest.OrbitCount)
            && error.ResourceKey == "HomeOrbitCountInvalid");
    }

    [Fact]
    public void About_validator_accepts_multiple_named_groups_with_skills()
    {
        var request = new AboutContentEditRequest(
            "Perfil español",
            null,
            [
                new SkillGroupEditModel(Guid.NewGuid(), "Desarrollo", null, 0,
                    [new SkillEditModel(Guid.NewGuid(), "C#", null, 0)]),
                new SkillGroupEditModel(Guid.NewGuid(), "Herramientas", null, 1,
                    [new SkillEditModel(Guid.NewGuid(), "Git", null, 0)])
            ],
            []);

        Assert.Empty(AboutContentValidator.Validate(request));
    }

    [Fact]
    public void Both_home_content_administration_pages_require_administrator_policy()
    {
        Assert.Equal(
            PortfolioAuthorization.AdministratorPolicy,
            typeof(HeroEditor).GetCustomAttribute<AuthorizeAttribute>()?.Policy);
        Assert.Equal(
            PortfolioAuthorization.AdministratorPolicy,
            typeof(AboutEditor).GetCustomAttribute<AuthorizeAttribute>()?.Policy);
    }

    [Fact]
    public void Model_contains_home_content_tables_with_localized_and_ordered_relations()
    {
        using var context = CreateFactory().CreateDbContext();
        var names = context.Model.GetEntityTypes().Select(entity => entity.ClrType.Name).ToArray();
        Assert.All(
            ["HeroContent", "HeroTranslation", "TechnologyLogo", "AboutProfile", "AboutProfileTranslation", "SkillGroup", "SkillGroupTranslation", "Skill", "SkillTranslation", "Hobby", "HobbyTranslation"],
            name => Assert.Contains(name, names));

        Assert.Equal(ValueGenerated.Never, context.Model.FindEntityType(typeof(HeroContent))!
            .FindProperty(nameof(HeroContent.Id))!.ValueGenerated);
        Assert.Equal(ValueGenerated.Never, context.Model.FindEntityType(typeof(AboutProfile))!
            .FindProperty(nameof(AboutProfile.Id))!.ValueGenerated);

        var translation = context.Model.FindEntityType(typeof(HobbyTranslation))!;
        Assert.Equal(
            [nameof(HobbyTranslation.HobbyId), nameof(HobbyTranslation.LanguageCode)],
            translation.FindPrimaryKey()!.Properties.Select(property => property.Name));
        Assert.Equal(DeleteBehavior.Cascade, translation.GetForeignKeys().Single().DeleteBehavior);
        var languageConstraint = context.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(HobbyTranslation))!.GetCheckConstraints()
            .Single(constraint => constraint.Name == "CK_HobbyTranslations_LanguageCode");
        Assert.Contains("es-ES", languageConstraint.Sql);
        Assert.Contains("en-US", languageConstraint.Sql);
    }

    private static TechnologyLogoEditModel Logo(int order, int orbit, string name)
    {
        var id = Guid.NewGuid();
        return new TechnologyLogoEditModel(
            id, name, $"Logo {name}", $"TechnologyLogos/{id:N}/{Guid.NewGuid():N}.png", "image/png", 8, order, orbit);
    }

    private static HobbyEditModel Hobby(
        int order,
        string spanishName,
        string spanishDescription,
        string? englishName,
        string? englishDescription)
    {
        var id = Guid.NewGuid();
        return new HobbyEditModel(
            id, $"Hobbies/{id:N}/{Guid.NewGuid():N}.png", "image/png", 8, order,
            spanishName, spanishDescription, englishName, englishDescription);
    }

    private static TestContextFactory CreateFactory() => new(Guid.NewGuid().ToString("N"));

    private sealed class FakeHomeContentImageStorageService : IHomeContentImageStorageService
    {
        public long MaximumFileSizeBytes => HeroContentValidator.MaximumImageSizeBytes;

        public List<(HomeContentImageKind Kind, Guid Id, string? Key)> Deleted { get; } = [];

        public Task<HomeContentImageUploadResult> UploadAsync(
            HomeContentImageKind kind, Guid entityId, string fileName, string contentType,
            long fileSize, Stream content, CancellationToken cancellationToken = default) =>
            Task.FromResult(new HomeContentImageUploadResult(HomeContentImageError.NotFound));

        public Task DeleteAsync(HomeContentImageKind kind, Guid entityId, string? storageKey)
        {
            Deleted.Add((kind, entityId, storageKey));
            return Task.CompletedTask;
        }

        public Task<HomeContentImageContent?> OpenPublicReadAsync(
            HomeContentImageKind kind, Guid entityId, CancellationToken cancellationToken = default) =>
            Task.FromResult<HomeContentImageContent?>(null);
    }

    private sealed class TestContextFactory(string databaseName) : IDbContextFactory<PortfolioDbContext>
    {
        private readonly DbContextOptions<PortfolioDbContext> options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        public PortfolioDbContext CreateDbContext() => new(options);
    }
}

public sealed class HomeContentImageStorageTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    private readonly TestContextFactory factory = new(Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task Upload_stores_opaque_image_outside_webroot_and_cleans_only_empty_entity_directory()
    {
        var service = CreateService();
        var id = Guid.NewGuid();
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var result = await service.UploadAsync(HomeContentImageKind.TechnologyLogo, id,
            "client-logo.png", "image/png", png.Length, new MemoryStream(png));

        Assert.Equal(HomeContentImageError.None, result.Error);
        Assert.NotNull(result.StorageKey);
        Assert.DoesNotContain("client-logo", result.StorageKey, StringComparison.Ordinal);
        Assert.True(Directory.Exists(Path.Combine(root, "App_Data", "HomeContentImages", "TechnologyLogos", id.ToString("N"))));

        await using (var context = factory.CreateDbContext())
        {
            context.TechnologyLogos.Add(new TechnologyLogo
            {
                Id = id,
                Name = "PNG",
                AlternativeText = "PNG logo",
                StorageKey = result.StorageKey,
                ContentType = result.ContentType!,
                SizeBytes = result.SizeBytes,
                DisplayOrder = 0,
                Orbit = 1
            });
            await context.SaveChangesAsync();
        }

        var image = await service.OpenPublicReadAsync(HomeContentImageKind.TechnologyLogo, id);
        Assert.NotNull(image);
        Assert.Equal("image/png", image.ContentType);
        await using (image.Content)
        {
            using var output = new MemoryStream();
            await image.Content.CopyToAsync(output);
            Assert.Equal(png, output.ToArray());
        }

        await service.DeleteAsync(HomeContentImageKind.TechnologyLogo, id, result.StorageKey);
        Assert.False(Directory.Exists(Path.Combine(root, "App_Data", "HomeContentImages", "TechnologyLogos", id.ToString("N"))));
        Assert.Null(await service.OpenPublicReadAsync(HomeContentImageKind.TechnologyLogo, id));
    }

    [Fact]
    public async Task Upload_accepts_safe_svg_and_rejects_executable_svg_and_mismatched_content()
    {
        var service = CreateService();
        var id = Guid.NewGuid();
        var valid = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?><svg xmlns=\"http://www.w3.org/2000/svg\"><style>.shape{fill:url(#gradient)}</style><defs><linearGradient id=\"gradient\"><stop offset=\"0%\" stop-color=\"#fff\"/></linearGradient></defs><path class=\"shape\" d=\"M0 0\"/></svg>");
        var safe = await service.UploadAsync(HomeContentImageKind.TechnologyLogo, id,
            "icon.svg", "image/svg+xml", valid.Length, new MemoryStream(valid));
        Assert.Equal(HomeContentImageError.None, safe.Error);

        var unsafeSvg = Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\"><style>.shape{fill:url(https://example.invalid/image.svg)}</style></svg>");
        var unsafeResult = await service.UploadAsync(HomeContentImageKind.Hobby, Guid.NewGuid(),
            "unsafe.svg", "image/svg+xml", unsafeSvg.Length, new MemoryStream(unsafeSvg));
        Assert.Equal(HomeContentImageError.InvalidContent, unsafeResult.Error);

        var executableSvg = Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\" onload=\"alert(1)\"><script>alert(1)</script></svg>");
        var executableResult = await service.UploadAsync(HomeContentImageKind.Hobby, Guid.NewGuid(),
            "executable.svg", "image/svg+xml", executableSvg.Length, new MemoryStream(executableSvg));
        Assert.Equal(HomeContentImageError.InvalidContent, executableResult.Error);

        var mismatch = await service.UploadAsync(HomeContentImageKind.Hobby, Guid.NewGuid(),
            "image.png", "image/svg+xml", valid.Length, new MemoryStream(valid));
        Assert.Equal(HomeContentImageError.UnsupportedType, mismatch.Error);
    }

    [Fact]
    public async Task Upload_accepts_azure_brand_svg_with_rdf_metadata()
    {
        var service = CreateService();
        var filePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "docs", "brand", "azure.svg"));
        var content = await File.ReadAllBytesAsync(filePath);
        var result = await service.UploadAsync(HomeContentImageKind.TechnologyLogo, Guid.NewGuid(),
            "azure.svg", "image/svg+xml", content.Length, new MemoryStream(content));

        Assert.Equal(HomeContentImageError.None, result.Error);
    }

    [Fact]
    public async Task Hero_profile_image_is_stored_and_served_only_for_the_active_profile_image_id()
    {
        var service = CreateService();
        var id = Guid.NewGuid();
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var uploaded = await service.UploadAsync(HomeContentImageKind.HeroProfile, id,
            "portrait.png", "image/png", png.Length, new MemoryStream(png));
        Assert.Equal(HomeContentImageError.None, uploaded.Error);

        await using (var context = factory.CreateDbContext())
        {
            context.HeroContents.Add(new HeroContent
            {
                Id = 1,
                OrbitCount = 1,
                ProfileImageId = id,
                ProfileImageStorageKey = uploaded.StorageKey,
                ProfileImageContentType = uploaded.ContentType,
                ProfileImageSizeBytes = uploaded.SizeBytes,
                ProfileImageAlternativeText = "Retrato"
            });
            await context.SaveChangesAsync();
        }

        var image = await service.OpenPublicReadAsync(HomeContentImageKind.HeroProfile, id);
        Assert.NotNull(image);
        Assert.Equal("image/png", image.ContentType);
        await image.Content.DisposeAsync();

        Assert.Null(await service.OpenPublicReadAsync(HomeContentImageKind.HeroProfile, Guid.NewGuid()));
        await service.DeleteAsync(HomeContentImageKind.HeroProfile, id, uploaded.StorageKey);
        Assert.False(File.Exists(Path.Combine(root, "App_Data", "HomeContentImages", "HeroProfiles", id.ToString("N"), Path.GetFileName(uploaded.StorageKey))));
    }

    [Fact]
    public async Task Upload_rejects_invalid_signatures_oversized_files_and_unsafe_delete_keys()
    {
        var service = CreateService(new HomeContentImageStorageOptions { Directory = "App_Data/Images", MaximumFileSizeBytes = 8 });
        var id = Guid.NewGuid();
        var invalid = await service.UploadAsync(HomeContentImageKind.Hobby, id,
            "bad.png", "image/png", 3, new MemoryStream([1, 2, 3]));
        var oversized = await service.UploadAsync(HomeContentImageKind.Hobby, id,
            "large.png", "image/png", 9, new MemoryStream(new byte[9]));
        Assert.Equal(HomeContentImageError.InvalidContent, invalid.Error);
        Assert.Equal(HomeContentImageError.TooLarge, oversized.Error);

        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var valid = await service.UploadAsync(HomeContentImageKind.Hobby, id,
            "good.png", "image/png", png.Length, new MemoryStream(png));
        Assert.Equal(HomeContentImageError.None, valid.Error);
        var fullPath = Path.Combine(root, "App_Data", "Images", valid.StorageKey!.Replace('/', Path.DirectorySeparatorChar));
        await service.DeleteAsync(HomeContentImageKind.Hobby, id, $"Hobbies/{id:N}/../{Path.GetFileName(fullPath)}");
        Assert.True(File.Exists(fullPath));
    }

    [Fact]
    public void Storage_refuses_a_path_inside_wwwroot()
    {
        var webRoot = Path.Combine(root, "wwwroot");
        var environment = new TestWebHostEnvironment { ContentRootPath = root, WebRootPath = webRoot };
        Assert.Throws<InvalidOperationException>(() => new HomeContentImageStorageService(
            factory,
            environment,
            Options.Create(new HomeContentImageStorageOptions { Directory = Path.Combine(webRoot, "images") }),
            NullLogger<HomeContentImageStorageService>.Instance));
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private HomeContentImageStorageService CreateService(HomeContentImageStorageOptions? options = null) =>
        new(factory,
            new TestWebHostEnvironment { ContentRootPath = root },
            Options.Create(options ?? new HomeContentImageStorageOptions()),
            NullLogger<HomeContentImageStorageService>.Instance);

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Portfolio.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
    }

    private sealed class TestContextFactory(string databaseName) : IDbContextFactory<PortfolioDbContext>
    {
        private readonly DbContextOptions<PortfolioDbContext> options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        public PortfolioDbContext CreateDbContext() => new(options);
    }
}