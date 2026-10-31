using AdotPet.Domain.Adopters;
using AdotPet.Domain.AdoptionRequests;
using AdotPet.Domain.Pets;
using AdotPet.Domain.Shelters;
using AdotPet.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdotPet.Infrastructure.Persistence;

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
