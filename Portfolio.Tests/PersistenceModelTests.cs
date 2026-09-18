using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure;
using Xunit;

namespace Portfolio.Tests;

public sealed class PersistenceModelTests
{
    [Fact]
    public void Model_contains_only_the_initial_portfolio_entities()
    {
        using var context = CreateContext();

        var entityNames = context.Model.GetEntityTypes()
            .Select(entityType => entityType.ClrType.Name)
            .OrderBy(name => name)
            .ToArray();

        Assert.Equal(
            ["BlogPost", "Certification", "Experience", "Project"],
            entityNames);
    }

    [Fact]
    public void Project_and_blog_post_slugs_are_unique()
    {
        using var context = CreateContext();

        var projectSlugIndex = context.Model.FindEntityType(typeof(Project))!
            .GetIndexes()
            .Single(index => index.Properties.Single().Name == nameof(Project.Slug));
        var blogPostSlugIndex = context.Model.FindEntityType(typeof(BlogPost))!
            .GetIndexes()
            .Single(index => index.Properties.Single().Name == nameof(BlogPost.Slug));

        Assert.True(projectSlugIndex.IsUnique);
        Assert.True(blogPostSlugIndex.IsUnique);
    }

    private static PortfolioDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PortfolioModelTests;Trusted_Connection=True;")
            .Options;

        return new PortfolioDbContext(options);
    }
}
