using AdotaPet.Domain.Pets;

namespace AdotaPet.Application.Pets.Dtos;

public sealed record PetSummaryDto(
    Guid Id, string Name, Species Species, Sex Sex, Size Size, AgeGroup AgeGroup,
    string? PhotoUrl, string City, string State, string ShelterName);
