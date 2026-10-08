using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Portfolio.Application.Projects;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Storage;
using Xunit;

namespace Portfolio.Tests;

public sealed class ProjectPreviewImageStorageTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    private readonly TestContextFactory factory = new(Guid.NewGuid().ToString());
    private readonly Project project = new() { Id = Guid.NewGuid() };
    private readonly ProjectPreviewImageStorageService service;

    public ProjectPreviewImageStorageTests()
    {
        Directory.CreateDirectory(root);
        using (var context = factory.CreateDbContext())
        {
            context.Projects.Add(project);
            context.SaveChanges();
        }

        var environment = new TestWebHostEnvironment { ContentRootPath = root };
        service = new ProjectPreviewImageStorageService(
            factory,
            environment,
            Options.Create(new ProjectPreviewImageStorageOptions { Directory = "preview-images" }),
            NullLogger<ProjectPreviewImageStorageService>.Instance);
    }

    [Fact]
    public async Task Upload_validates_png_signature_stores_opaquely_and_delete_clears_metadata()
    {
        var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var result = await service.UploadAsync(
            project.Id,
            "client-name.png",
            "image/png",
            pngHeader.Length,
            new MemoryStream(pngHeader));

        Assert.Equal(ProjectImageError.None, result.Error);
        Assert.NotNull(result.ImagePath);
        Assert.DoesNotContain("client-name", result.ImagePath, StringComparison.Ordinal);
        Assert.Equal(pngHeader.Length, await service.GetPreviewImageSizeAsync(project.Id));
        var content = await service.OpenPublicReadAsync(project.Id);
        Assert.NotNull(content);
        Assert.Equal("image/png", content.ContentType);
        using (content.Content)
        {
            Assert.Equal(pngHeader, await ReadAllAsync(content.Content));
        }

        Assert.True(await service.DeleteAsync(project.Id));
        Assert.Null(await service.GetPreviewImageSizeAsync(project.Id));
        await using var context = factory.CreateDbContext();
        Assert.Null((await context.Projects.SingleAsync(candidate => candidate.Id == project.Id)).PreviewImagePath);
    }

    [Fact]
    public async Task Replacing_preview_image_serves_the_new_file_and_removes_the_previous_one()
    {
        var firstBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1 };
        var secondBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 2 };
        var first = await service.UploadAsync(
            project.Id,
            "first.png",
            "image/png",
            firstBytes.Length,
            new MemoryStream(firstBytes));
        Assert.Equal(ProjectImageError.None, first.Error);
        Assert.NotNull(first.ImagePath);
        var firstStoredPath = Path.Combine(root, "preview-images", Path.GetFileName(first.ImagePath));

        var second = await service.UploadAsync(
            project.Id,
            "second.png",
            "image/png",
            secondBytes.Length,
            new MemoryStream(secondBytes));

        Assert.Equal(ProjectImageError.None, second.Error);
        Assert.NotEqual(first.ImagePath, second.ImagePath);
        Assert.False(File.Exists(firstStoredPath));

        var current = await service.OpenPublicReadAsync(project.Id);
        Assert.NotNull(current);
        using (current.Content)
        {
            Assert.Equal(secondBytes, await ReadAllAsync(current.Content));
        }
    }

    [Fact]
    public async Task Upload_rejects_invalid_signature_and_oversized_images()
    {
        var invalid = await service.UploadAsync(
            project.Id,
            "image.png",
            "image/png",
            4,
            new MemoryStream(new byte[] { 1, 2, 3, 4 }));
        var oversized = await service.UploadAsync(
            project.Id,
            "image.png",
            "image/png",
            service.MaximumFileSizeBytes + 1,
            Stream.Null);

        Assert.Equal(ProjectImageError.InvalidContent, invalid.Error);
        Assert.Equal(ProjectImageError.TooLarge, oversized.Error);
        Assert.Null(await service.OpenPublicReadAsync(project.Id));
    }

    [Fact]
    public async Task Upload_rejects_mime_extension_mismatch_and_missing_projects()
    {
        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var mismatch = await service.UploadAsync(project.Id, "image.jpg", "image/png", bytes.Length, new MemoryStream(bytes));
        var missing = await service.UploadAsync(Guid.NewGuid(), "image.png", "image/png", bytes.Length, new MemoryStream(bytes));

        Assert.Equal(ProjectImageError.UnsupportedType, mismatch.Error);
        Assert.Equal(ProjectImageError.ProjectNotFound, missing.Error);
    }

    [Fact]
    public void Storage_directory_cannot_be_inside_web_root()
    {
        var webRoot = Path.Combine(root, "wwwroot");
        var environment = new TestWebHostEnvironment { ContentRootPath = root, WebRootPath = webRoot };

        Assert.Throws<InvalidOperationException>(() => new ProjectPreviewImageStorageService(
            factory,
            environment,
            Options.Create(new ProjectPreviewImageStorageOptions
            {
                Directory = Path.Combine(webRoot, "project-images")
            }),
            NullLogger<ProjectPreviewImageStorageService>.Instance));
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
