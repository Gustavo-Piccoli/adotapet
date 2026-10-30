using AdotaPet.Application.Pets.Dtos;
using AdotaPet.Domain.Common;
using AdotaPet.Domain.Pets;
using AdotaPet.Domain.Shelters;

namespace AdotaPet.Application.Pets;

internal static class PetMapping
{
    public static string? PhotoUrl(Pet pet) =>
        pet.PhotoPath is null ? null : $"/api/pets/{pet.Id}/photo";

    public static Shelter ShelterOf(Pet pet) =>
        pet.Shelter ?? throw new InvalidOperationException("Pet loaded without its shelter.");

    public static PetSummaryDto ToSummary(Pet pet)
    {
        var shelter = ShelterOf(pet);
        return new PetSummaryDto(pet.Id, pet.Name, pet.Species, pet.Sex, pet.Size, pet.AgeGroup,
            PhotoUrl(pet), shelter.City, shelter.State, shelter.Name);
    }

    public static PetDetailsDto ToDetails(Pet pet)
    {
        var s = ShelterOf(pet);
        return new PetDetailsDto(pet.Id, pet.Name, pet.Species, pet.Sex, pet.Size, pet.AgeGroup,
            pet.IsNeutered, pet.IsVaccinated, pet.Description, PhotoUrl(pet), pet.Status,
            new ShelterInfoDto(s.Id, s.Name, s.City, s.State, s.Phone, s.Email, s.Description));
    }

    public static ShelterPetDto ToShelterPet(Pet pet) =>
        new(pet.Id, pet.Name, pet.Species, pet.Sex, pet.Size, pet.AgeGroup,
            pet.IsNeutered, pet.IsVaccinated, pet.Description, PhotoUrl(pet), pet.Status, pet.CreatedAt);

    public static NotFoundException PetNotFound() => new("Pet não encontrado.");
}
