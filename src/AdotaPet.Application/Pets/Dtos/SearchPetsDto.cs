using AdotaPet.Domain.Pets;

namespace AdotaPet.Application.Pets.Dtos;

public sealed record SearchPetsDto(
    Species? Species, Sex? Sex, Size? Size, AgeGroup? AgeGroup,
    string? State, string? City, int? Page, int? PageSize);
