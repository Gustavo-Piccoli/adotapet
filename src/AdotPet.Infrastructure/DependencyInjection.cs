using AdotPet.Application.Accounts;
using AdotPet.Application.Common;
using AdotPet.Application.Pets.Photos;
using AdotPet.Domain.Adopters;
using AdotPet.Domain.AdoptionRequests;
using AdotPet.Domain.Common;
using AdotPet.Domain.Pets;
using AdotPet.Domain.Shelters;
using AdotPet.Infrastructure.Identity;
using AdotPet.Infrastructure.Persistence;
using AdotPet.Infrastructure.Persistence.Repositories;
using AdotPet.Infrastructure.Photos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdotPet.Infrastructure;

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
