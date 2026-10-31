using AdotPet.Domain.Common;

namespace AdotPet.Application.Tests.Fakes;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCount { get; private set; }
    public int TransactionCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Simulates the retrying execution strategy: the action runs once, then this many more times, each time after
    /// <see cref="RollbackBeforeRetry"/> undoes the previous attempt's side effects (the real transaction rolls back).
    /// </summary>
    public int SimulatedRetries { get; set; }

    public Action? RollbackBeforeRetry { get; set; }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct)
    {
        TransactionCount++;
        var result = await action(ct);
        for (var retry = 0; retry < SimulatedRetries; retry++)
        {
            RollbackBeforeRetry?.Invoke();
            result = await action(ct);
        }
        return result;
    }
}
