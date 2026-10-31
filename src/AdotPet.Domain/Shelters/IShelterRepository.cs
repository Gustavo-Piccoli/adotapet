namespace AdotPet.Domain.Shelters;

public interface IShelterRepository
{
    Task<Shelter?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Shelter?> GetByUserIdAsync(string userId, CancellationToken ct);
    Task<bool> CnpjExistsAsync(string cnpj, CancellationToken ct);
    void Add(Shelter shelter);
}
