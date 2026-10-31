using AdotaPet.Domain.Pets;

namespace AdotaPet.Application.Pets.Dtos;

public sealed record PetInputDto(
    string Name, Species Species, Sex Sex, Size Size, AgeGroup AgeGroup,
    bool IsNeutered, bool IsVaccinated, string Description);
