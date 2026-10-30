using System.Reflection;
using AdotaPet.Domain.Common;
using AdotaPet.Domain.Pets;
using AdotaPet.Domain.Shelters;

namespace AdotaPet.Application.Tests.Fakes;

public sealed class InMemoryPetRepository : IPetRepository
{
    public List<Pet> Items { get; } = [];
    public PetSearchCriteria? LastCriteria { get; private set; }

    public Task<Pet?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(Items.FirstOrDefault(p => p.Id == id));

    public Task<PagedResult<Pet>> SearchAvailableAsync(PetSearchCriteria criteria, CancellationToken ct)
    {
        LastCriteria = criteria;
        var all = Items.Where(p => p.Status == PetStatus.Available).ToList();
        var page = all.Skip((criteria.Page - 1) * criteria.PageSize).Take(criteria.PageSize).ToList();
        return Task.FromResult(new PagedResult<Pet>(page, criteria.Page, criteria.PageSize, all.Count));
    }

    public Task<IReadOnlyList<Pet>> ListByShelterAsync(Guid shelterId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Pet>>(Items.Where(p => p.ShelterId == shelterId).ToList());

    public void Add(Pet pet) => Items.Add(pet);

    /// <summary>Creates a pet, attaches its shelter navigation (as an EF Include would) and stores it.</summary>
    public Pet Seed(Shelter shelter, string name = "Rex", string? photoPath = null, bool adopted = false)
    {
        var pet = Pet.Create(shelter.Id, name, Species.Dog, Sex.Male, Size.Medium, AgeGroup.Adult,
            true, true, "Dócil e brincalhão.", DateTimeOffset.UtcNow);
        typeof(Pet).GetProperty(nameof(Pet.Shelter), BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(pet, shelter);
        if (photoPath is not null) pet.SetPhoto(photoPath, DateTimeOffset.UtcNow);
        if (adopted) pet.MarkAdopted(DateTimeOffset.UtcNow);
        Items.Add(pet);
        return pet;
    }
}
