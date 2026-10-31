namespace AdotaPet.Domain.Common;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);

    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct);
}
