using AdotaPet.Domain.Adopters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdotaPet.Infrastructure.Persistence.Configurations;

public sealed class AdopterConfiguration : IEntityTypeConfiguration<Adopter>
{
    public void Configure(EntityTypeBuilder<Adopter> builder)
    {
        builder.ToTable("Adopters");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.UserId).IsRequired().HasMaxLength(450);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(80);
        builder.Property(a => a.Email).IsRequired().HasMaxLength(254);
        builder.Property(a => a.Phone).IsRequired().HasMaxLength(20);
        builder.Property(a => a.City).IsRequired().HasMaxLength(80);
        builder.Property(a => a.State).IsRequired().HasMaxLength(2);

        builder.HasIndex(a => a.UserId).IsUnique();
    }
}
