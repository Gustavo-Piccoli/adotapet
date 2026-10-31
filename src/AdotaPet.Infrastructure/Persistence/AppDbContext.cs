using AdotaPet.Domain.Adopters;
using AdotaPet.Domain.AdoptionRequests;
using AdotaPet.Domain.Pets;
using AdotaPet.Domain.Shelters;
using AdotaPet.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdotaPet.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Shelter> Shelters => Set<Shelter>();
    public DbSet<Adopter> Adopters => Set<Adopter>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<AdoptionRequest> AdoptionRequests => Set<AdoptionRequest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
