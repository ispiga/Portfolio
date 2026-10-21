using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Blog;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Services;
using Xunit;

namespace Portfolio.Tests;

public sealed class BlogPostContentSanitizerTests
{
    // ID compartido entre pruebas para rutas de imágenes.
    private static readonly Guid ImageId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("<svg onload=alert(1)><circle /></svg>")]
    [InlineData("<math><mtext><img src=x onerror=alert(1)></mtext></math>")]
    [InlineData("<iframe src=\"https://example.org\"></iframe>")]
    [InlineData("<object data=\"https://example.org\"></object><embed src=\"https://example.org\">")]
    [InlineData("<form action=\"https://example.org\"><input autofocus></form>")]
    [InlineData("<p onclick=\"alert(1)\" onmouseover=\"alert(2)\">Texto</p>")]
    [InlineData("<a href=\"javascript:alert(1)\">enlace</a>")]
    [InlineData("<a href=\"data:text/html,alert(1)\">enlace</a>")]
    [InlineData("<a href=\"vbscript:alert(1)\">enlace</a>")]
    [InlineData("<a href=\"//attacker.example/path\">enlace</a>")]
    [InlineData("<a href=\"\\\\attacker.example\\path\">enlace</a>")]
    [InlineData("<a href=\"file:///etc/passwd\">enlace</a>")]
    [InlineData("<img src=\"https://attacker.example/image.png\" onerror=\"alert(1)\">")]
    [InlineData("<img src=\"data:image/svg+xml,<svg onload=alert(1)>\">")]
    [InlineData("<p style=\"background-image:url(javascript:alert(1)); color:red; --x:1\" class=\"attacker\">Texto</p>")]
    public void Removes_active_elements_attributes_schemes_and_unneeded_styles(string input)
    {
        var sanitized = new BlogPostContentSanitizer().Sanitize(input);

        Assert.DoesNotContain("<script", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<svg", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<math", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<iframe", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<object", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<embed", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<form", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onload", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onerror", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onclick", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onmouseover", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("javascript:", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("vbscript:", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data:", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("attacker.example", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("background-image", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("color:", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("--x", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("class=", sanitized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Keeps_tinymce_formatting_links_images_alignment_and_tables()
    {
        const string input = "<h2>Heading</h2><p style=\"text-align: center;\"><strong>Bold</strong> <em>Italic</em> <u>Underlined</u></p>"
            + "<ol><li>One</li></ol><p><a href=\"https://example.org/article\">Link</a></p>"
            + "<p><img src=\"/blog-post-images/11111111-1111-1111-1111-111111111111\" alt=\"Alt\" width=\"320\" height=\"180\"></p>"
            + "<table style=\"border-collapse: collapse; width: 100%;\" border=\"1\"><tbody><tr><td style=\"text-align: center;\">Cell</td></tr></tbody></table>";

        var sanitizer = new BlogPostContentSanitizer();
        var sanitized = sanitizer.Sanitize(input);

        Assert.Contains("<h2>Heading</h2>", sanitized);
        Assert.Contains("<strong>Bold</strong>", sanitized);
        Assert.Contains("<em>Italic</em>", sanitized);
        Assert.Contains("<u>Underlined</u>", sanitized);
        Assert.Contains("<ol>", sanitized);
        Assert.Contains("href=\"https://example.org/article\"", sanitized);
        Assert.Contains("href=\"/blog\"", sanitizer.Sanitize("<a href=\"/blog\">Internal</a>"));
        Assert.Contains("href=\"mailto:contact@example.org\"", sanitizer.Sanitize("<a href=\"mailto:contact@example.org\">Email</a>"));
        Assert.Contains($"src=\"/blog-post-images/{ImageId:D}\"", sanitized);
        Assert.Contains("alt=\"Alt\"", sanitized);
        Assert.Contains("width=\"320\"", sanitized);
        Assert.Contains("height=\"180\"", sanitized);
        Assert.Contains("<table", sanitized);
        Assert.Contains("border=\"1\"", sanitized);
        Assert.Contains("border-collapse: collapse", sanitized);
        Assert.Contains("text-align: center", sanitized);
        Assert.DoesNotContain("<td style=\"border:", sanitized);
    }

    [Fact]
    public void Converts_only_blog_image_urls_for_public_and_administrative_contexts()
    {
        var sanitizer = new BlogPostContentSanitizer();
        var input = $"<p><img src=\"/blog-post-images/{ImageId:D}\"></p>";
        var legacyAdminInput = $"<p><img src=\"/admin/blog-post-images/{ImageId:D}\"></p>";

        Assert.Contains($"src=\"/blog-post-images/{ImageId:D}\"", sanitizer.Sanitize(input));
        Assert.Contains($"src=\"/admin/blog-post-images/{ImageId:D}\"", sanitizer.SanitizeForAdministration(input));
        Assert.Contains($"src=\"/admin/blog-post-images/{ImageId:D}\"", sanitizer.SanitizeForAdministration(legacyAdminInput));
        Assert.Contains($"src=\"/blog-post-images/{ImageId:D}\"", sanitizer.Sanitize($"<img src=\"/admin/blog-post-images/{ImageId:D}\">"));
        Assert.DoesNotContain("src=", sanitizer.Sanitize("<img src=\"/blog-post-images/not-a-guid\">"));
        Assert.DoesNotContain("src=", sanitizer.Sanitize("<img src=\"/blog-post-images/11111111-1111-1111-1111-111111111111/extra\">"));
        Assert.DoesNotContain("src=", sanitizer.Sanitize("<img src=\"https://attacker.example/image.png\">"));
        Assert.DoesNotContain("href=", sanitizer.Sanitize("<a href=\"javascript:alert(1)\">enlace</a>"));
        Assert.DoesNotContain("href=", sanitizer.Sanitize("<a href=\"//attacker.example/path\">enlace</a>"));
        Assert.DoesNotContain("href=", sanitizer.Sanitize("<a href=\"\\\\attacker.example\\path\">enlace</a>"));
        Assert.DoesNotContain("href=", sanitizer.Sanitize("<a href=\"file:///etc/passwd\">enlace</a>"));
    }

    [Fact]
    public void Keeps_only_safe_target_blank_links_and_is_idempotent()
    {
        var sanitizer = new BlogPostContentSanitizer();
        var first = sanitizer.Sanitize("<p><a href=\"https://example.org\" target=\"_blank\" rel=\"opener\">Link</a></p>");

        Assert.Contains("target=\"_blank\"", first);
        Assert.Contains("rel=\"noopener noreferrer\"", first);
        Assert.Equal(first, sanitizer.Sanitize(first));
    }

    [Theory]
    [InlineData("_self")]
    [InlineData("_parent")]
    [InlineData("_top")]
    [InlineData("named-frame")]
    public void Removes_non_blank_targets_and_rel(string target)
    {
        var sanitizer = new BlogPostContentSanitizer();

        var sanitized = sanitizer.Sanitize(
            $"<a href=\"https://example.org\" target=\"{target}\" rel=\"opener\">Link</a>");

        Assert.DoesNotContain("target=", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("rel=", sanitized, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("/blog", "href=\"/blog\"")]
    [InlineData("#section", "href=\"#section\"")]
    [InlineData("/%2f%2fattacker.example/path", "href=\"/%2f%2fattacker.example/path\"")]
    [InlineData("java%73cript:alert(1)", null)]
    [InlineData("%2f%2fattacker.example/path", null)]
    [InlineData("java&#x0A;script:alert(1)", null)]
    [InlineData("https://example.org\\@attacker.example/path", null)]
    public void Filters_urls_using_the_parsed_html_attribute_value(string href, string? expected)
    {
        var sanitized = new BlogPostContentSanitizer().Sanitize(
            $"<a href=\"{href}\">Link</a>");

        if (expected is null)
        {
            Assert.DoesNotContain("href=", sanitized, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            Assert.Contains(expected, sanitized, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Sanitization_is_idempotent_for_public_and_administrative_content()
    {
        var sanitizer = new BlogPostContentSanitizer();
        var content = $"<p><a href=\"https://example.org\" target=\"_blank\" rel=\"opener\">Link</a>"
            + $"<img src=\"/blog-post-images/{ImageId:D}\" alt=\"Image\"></p>";

        var publicContent = sanitizer.Sanitize(content);
        var administrativeContent = sanitizer.SanitizeForAdministration(content);

        Assert.Equal(publicContent, sanitizer.Sanitize(publicContent));
        Assert.Equal(administrativeContent, sanitizer.SanitizeForAdministration(administrativeContent));
        Assert.Contains($"src=\"/blog-post-images/{ImageId:D}\"", publicContent);
        Assert.Contains($"src=\"/admin/blog-post-images/{ImageId:D}\"", administrativeContent);
    }

    [Fact]
    public void Preserves_plain_text_and_handles_empty_input()
    {
        var sanitizer = new BlogPostContentSanitizer();

        Assert.Equal(string.Empty, sanitizer.Sanitize(null));
        Assert.Equal(string.Empty, sanitizer.Sanitize(string.Empty));
        Assert.Contains("Plain text", sanitizer.Sanitize("Plain text"));
    }

    [Fact]
    public async Task Save_and_admin_load_sanitize_content_and_use_admin_image_urls()
    {
        var factory = CreateFactory();
        var sanitizer = new BlogPostContentSanitizer();
        var service = new BlogPostAdministrationService(factory, sanitizer);
        var request = new BlogPostEditRequest(
            null,
            BlogPostEditorialStatus.ReadyToPublish,
            false,
            new("Artículo", "articulo-seguro", "Extracto", $"<p onclick=\"alert(1)\">Texto<img src=\"/blog-post-images/{ImageId:D}\"></p><script>alert(1)</script>"),
            new("", "", "", ""));

        var saved = await service.SaveAsync(request);
        Assert.True(saved.Succeeded);
        await using (var context = factory.CreateDbContext())
        {
            var persisted = await context.BlogPostTranslations.SingleAsync();
            Assert.DoesNotContain("onclick", persisted.Content, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("<script", persisted.Content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains($"src=\"/blog-post-images/{ImageId:D}\"", persisted.Content);
            Assert.Equal(sanitizer.Sanitize(request.Spanish.Content), persisted.Content);
        }

        var details = await service.GetBlogPostAsync(saved.BlogPostId!.Value);
        Assert.NotNull(details);
        Assert.Contains($"src=\"/admin/blog-post-images/{ImageId:D}\"", details.Spanish.Content);
        Assert.Equal(sanitizer.SanitizeForAdministration(request.Spanish.Content), details.Spanish.Content);
    }

    [Fact]
    public void Dependency_injection_registers_the_sanitizer_as_a_singleton()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddPortfolioPersistence(configuration);

        using var provider = services.BuildServiceProvider();
        var sanitizer = provider.GetRequiredService<IBlogPostContentSanitizer>();

        Assert.IsType<BlogPostContentSanitizer>(sanitizer);
        Assert.Same(sanitizer, provider.GetRequiredService<IBlogPostContentSanitizer>());
    }

    [Fact]
    public void Admin_image_urls_are_stable_when_loaded_and_saved_again()
    {
        var sanitizer = new BlogPostContentSanitizer();
        var editorContent = sanitizer.SanitizeForAdministration(
            $"<img src=\"/blog-post-images/{ImageId:D}\" alt=\"Imagen\">");
        var persistedContent = sanitizer.Sanitize(editorContent);
        var reloadedContent = sanitizer.SanitizeForAdministration(persistedContent);

        Assert.Contains($"src=\"/admin/blog-post-images/{ImageId:D}\"", editorContent);
        Assert.Contains($"src=\"/blog-post-images/{ImageId:D}\"", persistedContent);
        Assert.Contains($"src=\"/admin/blog-post-images/{ImageId:D}\"", reloadedContent);
        Assert.DoesNotContain("/admin/admin/", reloadedContent);
    }

    [Fact]
    public async Task Public_queries_sanitize_legacy_content_on_home_archive_and_detail_reads()
    {
        var factory = CreateFactory();
        var post = new BlogPost
        {
            Id = Guid.NewGuid(),
            EditorialStatus = BlogPostEditorialStatus.Published,
            PublishedOn = DateTimeOffset.UtcNow.AddMinutes(-1),
            Translations =
            [
                new BlogPostTranslation
                {
                    LanguageCode = "es-ES",
                    Title = "Artículo",
                    Slug = "articulo-legado",
                    Excerpt = "Resumen",
                    Content = "<p onmouseover=\"alert(1)\">Legado</p><svg onload=\"alert(2)\"><circle /></svg>"
                }
            ]
        };
        await using (var context = factory.CreateDbContext())
        {
            context.BlogPosts.Add(post);
            await context.SaveChangesAsync();
        }

        var query = new BlogPostQueryService(factory, new BlogPostContentSanitizer());
        var home = await query.GetHomePostsAsync();
        var archive = await query.GetPublishedPostsAsync();
        var detail = await query.GetPublishedPostBySlugAsync("articulo-legado");

        AssertSafeLegacyContent(home.Single().Content);
        AssertSafeLegacyContent(archive.Single().Content);
        AssertSafeLegacyContent(detail!.Content);
        await using var verify = factory.CreateDbContext();
        Assert.Contains("onmouseover", (await verify.BlogPostTranslations.SingleAsync()).Content);
    }

    private static void AssertSafeLegacyContent(string content)
    {
        Assert.Contains("Legado", content);
        Assert.DoesNotContain("onmouseover", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<svg", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onload", content, StringComparison.OrdinalIgnoreCase);
    }

    private static TestContextFactory CreateFactory() => new(Guid.NewGuid().ToString());

    private sealed class TestContextFactory(string databaseName) : IDbContextFactory<PortfolioDbContext>
    {
        private readonly DbContextOptions<PortfolioDbContext> options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        public PortfolioDbContext CreateDbContext() => new(options);
    }
}