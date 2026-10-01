using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
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
using Portfolio.Infrastructure.Identity;
using Portfolio.Infrastructure.Services;
using Portfolio.Infrastructure.Storage;

namespace Portfolio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPortfolioPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPortfolioDatabase(configuration);

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
        services.AddScoped<ICertificationAdministrationService, CertificationAdministrationService>();
        services.AddOptions<CertificationMediaStorageOptions>()
            .Bind(configuration.GetSection(CertificationMediaStorageOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => options.AllowedExtensions.Length > 0
                && options.AllowedExtensions.All(extension => extension is ".pdf" or ".jpg" or ".jpeg" or ".png"),
                "Only PDF, JPEG and PNG certification media are supported.")
            .ValidateOnStart();
        services.AddScoped<ICertificationMediaService, CertificationMediaStorageService>();
        services.AddScoped<IBlogPostQueryService, BlogPostQueryService>();
        services.AddScoped<IExperienceQueryService, ExperienceQueryService>();
        services.AddScoped<IExperienceAdministrationService, ExperienceAdministrationService>();
        services.AddOptions<ExperienceAttachmentStorageOptions>()
            .Bind(configuration.GetSection(ExperienceAttachmentStorageOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => options.AllowedExtensions.Length > 0
                && options.AllowedExtensions.All(extension => extension is ".pdf" or ".jpg" or ".jpeg" or ".png"),
                "Only PDF, JPEG and PNG experience attachments are supported.")
            .ValidateOnStart();
        services.AddScoped<IExperienceAttachmentService, ExperienceAttachmentStorageService>();
        services.AddScoped<IProjectQueryService, ProjectQueryService>();
        services.AddScoped<IProjectAdministrationService, ProjectAdministrationService>();
        services.AddOptions<ProjectPreviewImageStorageOptions>()
            .Bind(configuration.GetSection(ProjectPreviewImageStorageOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddScoped<IProjectPreviewImageService, ProjectPreviewImageStorageService>();

        return services;
    }

    public static IServiceCollection AddPortfolioDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Portfolio");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContextFactory<PortfolioDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                    sqlOptions.MigrationsAssembly(typeof(PortfolioDbContext).Assembly.FullName)));
        }

        return services;
    }

    public static IServiceCollection AddPortfolioIdentity(this IServiceCollection services)
    {
        AddPortfolioIdentityStores(services)
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddAuthorization(options =>
            options.AddPolicy(PortfolioAuthorization.AdministratorPolicy, policy =>
                policy.RequireRole(PortfolioAuthorization.AdministratorRole)));

        return services;
    }

    public static IServiceCollection AddPortfolioProvisioningIdentity(this IServiceCollection services)
    {
        AddPortfolioIdentityStores(services);
        return services;
    }

    private static IdentityBuilder AddPortfolioIdentityStores(IServiceCollection services)
    {
        return services.AddIdentityCore<PortfolioUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<PortfolioDbContext>();
    }
}