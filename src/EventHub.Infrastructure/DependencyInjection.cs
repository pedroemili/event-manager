using EventHub.Application.Common.Interfaces;
using EventHub.Application.Common.Interfaces.Services;
using EventHub.Infrastructure.Persistence;
using EventHub.Infrastructure.Persistence.Initialization;
using EventHub.Infrastructure.Repositories;
using EventHub.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<EventHubDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(EventHubDbContext).Assembly.FullName);
                npgsqlOptions.CommandTimeout(30);
            });
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IVenueRepository, VenueRepository>();

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<QRCodeSettings>(configuration.GetSection("QRCode"));
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IQRCodeService, QRCodeService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<PostgresDatabaseInitializer>();
        services.AddScoped<DeveloperSeedEnricher>();

        return services;
    }
}

