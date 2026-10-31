using AdotaPet.Domain.Adopters;
using AdotaPet.Domain.Common;
using AdotaPet.Domain.Pets;

namespace AdotaPet.Domain.AdoptionRequests;

public sealed class AdoptionRequest
{
    private AdoptionRequest() { }

    public Guid Id { get; private set; }
    public Guid PetId { get; private set; }
    public Pet? Pet { get; private set; }
    public Guid AdopterId { get; private set; }
    public Adopter? Adopter { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public AdoptionRequestStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? DecidedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public static AdoptionRequest Create(Pet pet, Guid adopterId, string message, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(pet);
        if (pet.Status != PetStatus.Available)
            throw new ConflictException("Este pet não está mais disponível para adoção.");

        return new AdoptionRequest
        {
            Id = Guid.NewGuid(),
            PetId = pet.Id,
            Pet = pet,
            AdopterId = adopterId,
            Message = Guard.Text(message, "Mensagem", 10, 1000),
            Status = AdoptionRequestStatus.Pending,
            CreatedAt = now,
        };
    }

    public void Approve(DateTimeOffset now) => Decide(AdoptionRequestStatus.Approved, now);

    public void Reject(DateTimeOffset now) => Decide(AdoptionRequestStatus.Rejected, now);

    private void Decide(AdoptionRequestStatus status, DateTimeOffset now)
    {
        if (Status != AdoptionRequestStatus.Pending)
            throw new ConflictException("Este pedido já foi decidido.");
        Status = status;
        DecidedAt = now;
    }
}
