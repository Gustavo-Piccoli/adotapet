using AdotaPet.Application.Accounts;
using AdotaPet.Application.Common;
using AdotaPet.Application.Pets.Photos;
using AdotaPet.Domain.Adopters;
using AdotaPet.Domain.AdoptionRequests;
using AdotaPet.Domain.Common;
using AdotaPet.Domain.Pets;
using AdotaPet.Domain.Shelters;
using AdotaPet.Infrastructure.Identity;
using AdotaPet.Infrastructure.Persistence;
using AdotaPet.Infrastructure.Persistence.Repositories;
using AdotaPet.Infrastructure.Photos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdotaPet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sql => sql.EnableRetryOnFailure()));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => o.SigningKey is { Length: >= 32 }, "Jwt:SigningKey must be at least 32 characters.")
            .ValidateOnStart();
        services.Configure<PhotoStorageOptions>(options =>
        {
            var path = configuration["Storage:PhotosPath"];
            if (!string.IsNullOrWhiteSpace(path)) options.RootPath = path;
        });

        services.AddScoped<IShelterRepository, ShelterRepository>();
        services.AddScoped<IAdopterRepository, AdopterRepository>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<IAdoptionRequestRepository, AdoptionRequestRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IPhotoStorage, LocalPhotoStorage>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
