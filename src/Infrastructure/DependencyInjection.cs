using System.Text;
using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Caching;
using Nesto.Application.Abstractions.Data;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Apartments.Search;
using Nesto.Application.Bookings;
using Nesto.Application.Reviews;
using Nesto.Application.Users;
using Nesto.Domain.Apartments;
using Nesto.Domain.Bookings;
using Nesto.Domain.Reviews;
using Nesto.Domain.Users;
using Nesto.Infrastructure.Apartments;
using Nesto.Infrastructure.Authentication;
using Nesto.Infrastructure.Authorization;
using Nesto.Infrastructure.Bookings;
using Nesto.Infrastructure.Caching;
using Nesto.Infrastructure.Database;
using Nesto.Infrastructure.DomainEvents;
using Nesto.Infrastructure.Identity;
using Nesto.Infrastructure.IntegrationEvents;
using Nesto.Infrastructure.Outbox;
using Nesto.Infrastructure.Reviews;
using Nesto.Infrastructure.Time;
using Nesto.Infrastructure.Users;
using Npgsql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nesto.SharedKernel;

namespace Nesto.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddHealthChecks(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal();

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        services.Configure<OutboxOptions>(options =>
        {
            options.IntervalSeconds = 5;
            options.BatchSize = 50;
            options.MaxAttempts = 10;
        });
        services.AddHostedService<OutboxProcessor>();
        services.AddHostedService<IntegrationOutboxProcessor>();
        services.AddScoped<IIntegrationEventPublisher, OutboxIntegrationEventPublisher>();
        services.AddScoped<IIntegrationEventTransport, LoggingIntegrationEventTransport>();

#pragma warning disable EXTEXP0018 // HybridCache is released; the API is stable in .NET 10.
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new()
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(1)
            };
        });
#pragma warning restore EXTEXP0018
        services.AddSingleton<ICacheService, HybridCacheService>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Database")!;

        services.AddDbContext<ApplicationDbContext>(
            options => options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
                .UseSnakeCaseNamingConvention());

        services.AddIdentityCore<IdentityAccount>(options =>
            {
                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<IIdentityService, IdentityService>();

        string? cacheConnectionString = configuration.GetConnectionString("Cache");
        if (!string.IsNullOrWhiteSpace(cacheConnectionString))
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = cacheConnectionString);
        }

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        DapperTypeHandlers.Register();

        services.AddSingleton(_ => new NpgsqlDataSourceBuilder(connectionString).Build());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IApartmentRepository, ApartmentRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IUserReadService, UserReadService>();
        services.AddScoped<IApartmentReadService, ApartmentReadService>();
        services.AddScoped<IBookingReadService, BookingReadService>();
        services.AddScoped<IReviewReadService, ReviewReadService>();

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Database")!);

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<ITokenProvider, TokenProvider>();

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();

        services.AddScoped<PermissionProvider>();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }
}
