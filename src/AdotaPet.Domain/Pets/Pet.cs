using AdotaPet.Domain.Common;
using AdotaPet.Domain.Shelters;

namespace AdotaPet.Domain.Pets;

public sealed class Pet
{
    private Pet() { }

    public Guid Id { get; private set; }
    public Guid ShelterId { get; private set; }
    public Shelter? Shelter { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Species Species { get; private set; }
    public Sex Sex { get; private set; }
    public Size Size { get; private set; }
    public AgeGroup AgeGroup { get; private set; }
    public bool IsNeutered { get; private set; }
    public bool IsVaccinated { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? PhotoPath { get; private set; }
    public PetStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public static Pet Create(Guid shelterId, string name, Species species, Sex sex, Size size,
        AgeGroup ageGroup, bool isNeutered, bool isVaccinated, string description, DateTimeOffset now)
    {
        var pet = new Pet
        {
            Id = Guid.NewGuid(),
            ShelterId = shelterId,
            Status = PetStatus.Available,
            CreatedAt = now,
        };
        pet.Apply(name, species, sex, size, ageGroup, isNeutered, isVaccinated, description, now);
        return pet;
    }

    public void Update(string name, Species species, Sex sex, Size size, AgeGroup ageGroup,
        bool isNeutered, bool isVaccinated, string description, DateTimeOffset now) =>
        Apply(name, species, sex, size, ageGroup, isNeutered, isVaccinated, description, now);

    public void SetPhoto(string photoPath, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(photoPath))
            throw new DomainValidationException("Foto inválida.");
        PhotoPath = photoPath;
        UpdatedAt = now;
    }

    public void MarkAdopted(DateTimeOffset now)
    {
        if (Status == PetStatus.Adopted)
            throw new ConflictException("Este pet já foi adotado.");
        Status = PetStatus.Adopted;
        UpdatedAt = now;
    }

    private void Apply(string name, Species species, Sex sex, Size size, AgeGroup ageGroup,
        bool isNeutered, bool isVaccinated, string description, DateTimeOffset now)
    {
        var validName = Guard.Text(name, "Nome", 2, 60);
        var validDescription = Guard.Text(description, "Descrição", 1, 2000);
        var validSpecies = Guard.Defined(species, "Espécie");
        var validSex = Guard.Defined(sex, "Sexo");
        var validSize = Guard.Defined(size, "Porte");
        var validAgeGroup = Guard.Defined(ageGroup, "Faixa etária");

        Name = validName;
        Description = validDescription;
        Species = validSpecies;
        Sex = validSex;
        Size = validSize;
        AgeGroup = validAgeGroup;
        IsNeutered = isNeutered;
        IsVaccinated = isVaccinated;
        UpdatedAt = now;
    }
}
