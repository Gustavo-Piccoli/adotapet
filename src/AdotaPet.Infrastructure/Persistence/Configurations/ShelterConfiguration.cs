using AdotaPet.Domain.Shelters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdotaPet.Infrastructure.Persistence.Configurations;

public sealed class ShelterConfiguration : IEntityTypeConfiguration<Shelter>
{
    public void Configure(EntityTypeBuilder<Shelter> builder)
    {
        builder.ToTable("Shelters");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.UserId).IsRequired().HasMaxLength(450);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(80);
        builder.Property(s => s.Cnpj).IsRequired().HasMaxLength(14);
        builder.Property(s => s.City).IsRequired().HasMaxLength(80);
        builder.Property(s => s.State).IsRequired().HasMaxLength(2);
        builder.Property(s => s.Phone).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Email).IsRequired().HasMaxLength(254);
        builder.Property(s => s.Description).IsRequired().HasMaxLength(1000);

        builder.HasIndex(s => s.Cnpj).IsUnique();
        builder.HasIndex(s => s.UserId).IsUnique();
        builder.HasIndex(s => new { s.State, s.City });
    }
}
