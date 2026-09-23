using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Certifications;
using Portfolio.Application.Blog;
using Portfolio.Application.Experiences;
using Portfolio.Application.Projects;
using Portfolio.Infrastructure.Services;

namespace Portfolio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPortfolioPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Portfolio");
        services.AddScoped<ICertificationQueryService, CertificationQueryService>();
        services.AddScoped<IBlogPostQueryService, BlogPostQueryService>();
        services.AddScoped<IExperienceQueryService, ExperienceQueryService>();
        services.AddScoped<IProjectQueryService, ProjectQueryService>();

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContextFactory<PortfolioDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                    sqlOptions.MigrationsAssembly(typeof(PortfolioDbContext).Assembly.FullName)));
        }

        return services;
    }
}