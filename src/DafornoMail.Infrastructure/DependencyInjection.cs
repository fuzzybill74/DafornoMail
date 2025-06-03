// DafornoMail.Infrastructure/DependencyInjection.cs
using DafornoMail.Core.Interfaces;
using DafornoMail.Core.Interfaces.Repositories;
using DafornoMail.Core.Interfaces.Services;
using DafornoMail.Infrastructure.Data;
using DafornoMail.Infrastructure.Email.Providers;
using DafornoMail.Infrastructure.Repositories;
using DafornoMail.Infrastructure.Security;
using DafornoMail.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DafornoMail.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped<IEmailAccountRepository, EmailAccountRepository>();
        services.AddScoped<IEmailFolderRepository, EmailFolderRepository>();
        services.AddScoped<IEmailMessageRepository, EmailMessageRepository>();


        // Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<IKeyVaultService, KeyVaultService>();

        // Email Providers
        services.AddTransient<GmailProvider>();
        // Add other providers...

        return services;
    }
}