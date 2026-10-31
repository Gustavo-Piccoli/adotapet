using AdotPet.Domain.Pets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdotPet.Infrastructure.Persistence.Configurations;

public sealed class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable("Pets");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Name).IsRequired().HasMaxLength(60);
        builder.Property(p => p.Description).IsRequired().HasMaxLength(2000);
        builder.Property(p => p.PhotoPath).HasMaxLength(100);
        builder.Property(p => p.Species).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Sex).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Size).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.AgeGroup).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.RowVersion).IsRowVersion();

        builder.HasOne(p => p.Shelter)
            .WithMany()
            .HasForeignKey(p => p.ShelterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.ShelterId);
        // State and City belong to the Shelter (searched through a join), so Pets are indexed
        // by Status + CreatedAt, which matches the listing filter and ordering.
        builder.HasIndex(p => new { p.Status, p.CreatedAt });
    }
}
