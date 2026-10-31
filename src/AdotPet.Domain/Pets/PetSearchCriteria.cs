namespace AdotPet.Domain.Pets;

public sealed record PetSearchCriteria(
    Species? Species, Sex? Sex, Size? Size, AgeGroup? AgeGroup,
    string? State, string? City, int Page, int PageSize);
