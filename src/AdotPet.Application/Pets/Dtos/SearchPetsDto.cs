using AdotPet.Domain.Pets;

namespace AdotPet.Application.Pets.Dtos;

public sealed record SearchPetsDto(
    Species? Species, Sex? Sex, Size? Size, AgeGroup? AgeGroup,
    string? State, string? City, int? Page, int? PageSize);
