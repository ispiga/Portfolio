using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Portfolio.Application.Blog;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Storage;
using Xunit;

namespace Portfolio.Tests;

public sealed class BlogPostImageStorageTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    private readonly Guid blogPostId = Guid.NewGuid();
    private readonly TestContextFactory factory;
    private readonly BlogPostImageStorageService service;

    public BlogPostImageStorageTests()
    {
        Directory.CreateDirectory(root);
        factory = new TestContextFactory(Guid.NewGuid().ToString());
        using (var context = factory.CreateDbContext())
        {
            context.BlogPosts.Add(new BlogPost
            {
                Id = blogPostId,
                EditorialStatus = BlogPostEditorialStatus.Draft,
                Translations = [new BlogPostTranslation
                {
                    LanguageCode = "es-ES",
                    Title = "Artículo",
                    Slug = "articulo",
                    Excerpt = "Resumen",
                    Content = "Contenido"
                }]
            });
            context.SaveChanges();
        }

        var environment = new TestWebHostEnvironment(root);
        service = new BlogPostImageStorageService(
            factory,
            environment,
            Options.Create(new BlogPostImageStorageOptions
            {
                Directory = "blog-images",
                MaximumFileSizeBytes = 4096
            }));
    }

    [Fact]
    public async Task Upload_stores_opaque_file_in_per_post_directory_and_public_read_requires_published_post()
    {
        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var result = await service.UploadImageAsync(blogPostId, "original-name.png", "image/png", bytes.Length, new MemoryStream(bytes));

        Assert.True(result.Succeeded);
        var image = Assert.IsType<BlogPostImageReadModel>(result.Image);
        var images = await service.GetImagesAsync(blogPostId);
        Assert.Equal(image, Assert.Single(images));
        Assert.Equal(bytes.Length, image.SizeBytes);

        await using var context = factory.CreateDbContext();
        var storageKey = await context.BlogPostImages.Where(candidate => candidate.Id == image.Id)
            .Select(candidate => candidate.StorageKey).SingleAsync();
        Assert.StartsWith($"posts/{blogPostId:N}/", storageKey, StringComparison.Ordinal);
        Assert.DoesNotContain("original-name", storageKey, StringComparison.Ordinal);
        var path = Path.Combine(root, "blog-images", storageKey.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(path));
        Assert.False(path.StartsWith(Path.Combine(root, "wwwroot"), StringComparison.OrdinalIgnoreCase));
        Assert.Null(await service.OpenImageAsync(image.Id, administratorCanViewDrafts: false));

        var draftPreview = await service.OpenImageAsync(image.Id, administratorCanViewDrafts: true);
        Assert.NotNull(draftPreview);
        await using (draftPreview.Content)
        {
            Assert.Equal(bytes, await ReadAllAsync(draftPreview.Content));
        }

        await using (var update = factory.CreateDbContext())
        {
            var post = await update.BlogPosts.SingleAsync(candidate => candidate.Id == blogPostId);
            post.EditorialStatus = BlogPostEditorialStatus.Published;
            post.PublishedOn = DateTimeOffset.UtcNow;
            await update.SaveChangesAsync();
        }

        var publicContent = await service.OpenImageAsync(image.Id, administratorCanViewDrafts: false);
        Assert.NotNull(publicContent);
        await using (publicContent.Content)
        {
            Assert.Equal(bytes, await ReadAllAsync(publicContent.Content));
        }
    }

    [Fact]
    public async Task Svg_is_allowed_only_for_safe_xml_and_upload_checks_extension_mime_and_size()
    {
        var safeSvg = System.Text.Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 10 10\"><rect width=\"10\" height=\"10\"/></svg>");
        var safe = await service.UploadImageAsync(blogPostId, "shape.svg", "image/svg+xml", safeSvg.Length, new MemoryStream(safeSvg));
        Assert.True(safe.Succeeded);
        Assert.Equal("image/svg+xml", safe.Image!.ContentType);

        var maliciousSvg = System.Text.Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\"><script>alert(1)</script></svg>");
        var malicious = await service.UploadImageAsync(blogPostId, "unsafe.svg", "image/svg+xml", maliciousSvg.Length, new MemoryStream(maliciousSvg));
        Assert.Equal(BlogPostImageError.InvalidContent, malicious.Error);

        var mismatched = await service.UploadImageAsync(blogPostId, "shape.png", "image/svg+xml", safeSvg.Length, new MemoryStream(safeSvg));
        Assert.Equal(BlogPostImageError.UnsupportedType, mismatched.Error);

        var oversized = await service.UploadImageAsync(blogPostId, "large.png", "image/png", 4097, new MemoryStream(new byte[4097]));
        Assert.Equal(BlogPostImageError.TooLarge, oversized.Error);
    }

    [Fact]
    public async Task Image_delete_is_blocked_when_referenced_and_removes_featured_image_after_content_is_saved_without_it()
    {
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var uploaded = await service.UploadImageAsync(blogPostId, "image.png", "image/png", png.Length, new MemoryStream(png));
        var image = uploaded.Image!;
        await service.SetFeaturedImageAsync(image.Id);
        var publicUrl = $"/blog-post-images/{image.Id}";

        await using (var context = factory.CreateDbContext())
        {
            var translation = await context.BlogPostTranslations.SingleAsync(candidate => candidate.BlogPostId == blogPostId);
            translation.Content = $"<p><img src=\"{publicUrl}\" /></p>";
            await context.SaveChangesAsync();
        }

        Assert.Equal(BlogPostImageError.InUse, await service.DeleteImageAsync(image.Id));
        await using (var context = factory.CreateDbContext())
        {
            var translation = await context.BlogPostTranslations.SingleAsync(candidate => candidate.BlogPostId == blogPostId);
            translation.Content = "<p>Sin imagen</p>";
            await context.SaveChangesAsync();
        }

        var storedPath = Path.Combine(root, "blog-images", $"posts/{blogPostId:N}/{image.Id:N}.png");
        Assert.True(File.Exists(storedPath));
        Assert.Equal(BlogPostImageError.None, await service.DeleteImageAsync(image.Id));
        Assert.False(File.Exists(storedPath));
        await using var check = factory.CreateDbContext();
        Assert.Null(await check.BlogPostImages.FindAsync(image.Id));
        Assert.Null((await check.BlogPosts.SingleAsync(post => post.Id == blogPostId)).FeaturedImagePath);
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<byte[]> ReadAllAsync(Stream stream)
    {
        using var output = new MemoryStream();
        await stream.CopyToAsync(output);
        return output.ToArray();
    }

    private sealed class TestWebHostEnvironment(string contentRoot) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Portfolio.Tests";
        public string ContentRootPath { get; set; } = contentRoot;
        public string EnvironmentName { get; set; } = "Development";
        public string WebRootPath { get; set; } = Path.Combine(contentRoot, "wwwroot");
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class TestContextFactory(string databaseName) : IDbContextFactory<PortfolioDbContext>
    {
        private readonly DbContextOptions<PortfolioDbContext> options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        public PortfolioDbContext CreateDbContext() => new(options);
    }
}
