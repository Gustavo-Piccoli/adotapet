using AdotPet.Domain.Common;
using AdotPet.Domain.Pets;

namespace AdotPet.Domain.Tests.Pets;

public class PetTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);

    private static Pet Create(Species species = Species.Dog) =>
        Pet.Create(Guid.NewGuid(), " Rex ", species, Sex.Male, Size.Medium, AgeGroup.Adult,
            true, true, "Cão muito dócil", Now);

    [Fact]
    public void Create_Valid_IsAvailableWithoutPhoto()
    {
        var pet = Create();

        Assert.Equal("Rex", pet.Name);
        Assert.Equal(PetStatus.Available, pet.Status);
        Assert.Null(pet.PhotoPath);
        Assert.NotEqual(Guid.Empty, pet.Id);
        Assert.Equal(Now, pet.CreatedAt);
        Assert.Equal(Now, pet.UpdatedAt);
    }

    [Fact]
    public void Update_ChangesFieldsAndUpdatedAt()
    {
        var pet = Create();
        var later = Now.AddHours(1);

        pet.Update("Mia", Species.Cat, Sex.Female, Size.Small, AgeGroup.Puppy, false, false, "Gata brincalhona", later);

        Assert.Equal("Mia", pet.Name);
        Assert.Equal(Species.Cat, pet.Species);
        Assert.Equal(Sex.Female, pet.Sex);
        Assert.Equal(Size.Small, pet.Size);
        Assert.Equal(AgeGroup.Puppy, pet.AgeGroup);
        Assert.False(pet.IsNeutered);
        Assert.False(pet.IsVaccinated);
        Assert.Equal("Gata brincalhona", pet.Description);
        Assert.Equal(later, pet.UpdatedAt);
        Assert.Equal(Now, pet.CreatedAt);
    }

    [Fact]
    public void MarkAdopted_SetsAdopted()
    {
        var pet = Create();

        pet.MarkAdopted(Now.AddDays(1));

        Assert.Equal(PetStatus.Adopted, pet.Status);
    }

    [Fact]
    public void MarkAdopted_WhenAlreadyAdopted_ThrowsConflict()
    {
        var pet = Create();
        pet.MarkAdopted(Now);

        var ex = Assert.Throws<ConflictException>(() => pet.MarkAdopted(Now));
        Assert.Equal(ConflictCodes.PetAlreadyAdopted, ex.Code);
    }

    [Fact]
    public void Create_WithUndefinedEnum_Throws() =>
        Assert.Throws<DomainValidationException>(() => Create((Species)99));
}
