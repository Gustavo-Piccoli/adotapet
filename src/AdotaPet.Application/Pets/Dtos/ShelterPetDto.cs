using AdotaPet.Domain.Pets;

namespace AdotaPet.Application.Pets.Dtos;

public sealed record ShelterPetDto(
    Guid Id, string Name, Species Species, Sex Sex, Size Size, AgeGroup AgeGroup,
    bool IsNeutered, bool IsVaccinated, string Description, string? PhotoUrl,
    PetStatus Status, DateTimeOffset CreatedAt);
