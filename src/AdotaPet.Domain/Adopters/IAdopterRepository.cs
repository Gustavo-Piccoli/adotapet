namespace AdotaPet.Domain.Adopters;

public interface IAdopterRepository
{
    Task<Adopter?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Adopter?> GetByUserIdAsync(string userId, CancellationToken ct);
    void Add(Adopter adopter);
}
