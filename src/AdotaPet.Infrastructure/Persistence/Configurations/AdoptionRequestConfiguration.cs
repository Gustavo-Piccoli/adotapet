using AdotaPet.Domain.AdoptionRequests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdotaPet.Infrastructure.Persistence.Configurations;

public sealed class AdoptionRequestConfiguration : IEntityTypeConfiguration<AdoptionRequest>
{
    public const string PendingUniqueIndexName = "IX_AdoptionRequests_AdopterId_PetId_Pending";

    public void Configure(EntityTypeBuilder<AdoptionRequest> builder)
    {
        builder.ToTable("AdoptionRequests");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.Message).IsRequired().HasMaxLength(1000);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.RowVersion).IsRowVersion();

        builder.HasOne(r => r.Pet)
            .WithMany()
            .HasForeignKey(r => r.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Adopter)
            .WithMany()
            .HasForeignKey(r => r.AdopterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.AdopterId, r.PetId })
            .IsUnique()
            .HasFilter("[Status] = 'Pending'")
            .HasDatabaseName(PendingUniqueIndexName);

        builder.HasIndex(r => new { r.PetId, r.Status });
    }
}
