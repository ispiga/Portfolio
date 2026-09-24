using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Portfolio.Application.Contact;
using Portfolio.Application.Email;
using Portfolio.Application.Certifications;
using Portfolio.Application.Blog;
using Portfolio.Application.Experiences;
using Portfolio.Application.Projects;
using Portfolio.Infrastructure.Email;
using Portfolio.Infrastructure.Services;

namespace Portfolio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPortfolioPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Portfolio");
        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.OAuth.ClientId)
                && !string.IsNullOrWhiteSpace(options.OAuth.ClientSecret)
                && !string.IsNullOrWhiteSpace(options.OAuth.RefreshToken),
                "Gmail OAuth2 client credentials and refresh token are required.")
            .ValidateOnStart();
        services.AddSingleton<IContactEmailConfiguration>(serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<SmtpOptions>>().Value);
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IEmailService, MailKitEmailService>();
        services.AddHttpClient<IGmailOAuthTokenService, GmailOAuthTokenService>();
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