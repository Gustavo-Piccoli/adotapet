using AdotPet.Domain.Pets;

namespace AdotPet.Application.Pets.Dtos;

public sealed record ShelterInfoDto(
    Guid Id, string Name, string City, string State, string Phone, string Email, string Description);

public sealed record PetDetailsDto(
    Guid Id, string Name, Species Species, Sex Sex, Size Size, AgeGroup AgeGroup,
    bool IsNeutered, bool IsVaccinated, string Description, string? PhotoUrl,
    PetStatus Status, ShelterInfoDto Shelter);
