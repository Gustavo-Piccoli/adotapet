using AdotaPet.Domain.Common;
using AdotaPet.Infrastructure.Persistence.Configurations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AdotaPet.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (Translate(ex) is { } translated)
        {
            throw translated;
        }
    }

    public Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct)
    {
        // The retrying execution strategy forbids user transactions unless they run inside it.
        var strategy = db.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async token =>
        {
            // Every attempt (including retries) must start from a clean tracker: entities from a failed
            // attempt are stale. Callers must therefore load/create their entities inside the action (never capture
            // entities built outside it), so that a retry rebuilds them.
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(token);
            try
            {
                var result = await action(token);
                await transaction.CommitAsync(token);
                return result;
            }
            catch (DbUpdateException ex) when (Translate(ex) is { } translated)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw translated;
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        }, ct);
    }

    private static ConflictException? Translate(DbUpdateException ex)
    {
        if (ex is DbUpdateConcurrencyException)
            return new ConflictException("Este pedido já foi decidido por outra pessoa. Atualize a página.", ConflictCodes.ConcurrencyConflict);

        if (ex.GetBaseException() is SqlException { Number: UniqueIndexViolation or UniqueConstraintViolation } sql)
        {
            return sql.Message.Contains(AdoptionRequestConfiguration.PendingUniqueIndexName, StringComparison.Ordinal)
                ? new ConflictException("Você já tem um pedido em análise para este pet.", ConflictCodes.DuplicatePendingRequest)
                : new ConflictException("Registro duplicado.", ConflictCodes.DuplicateRecord);
        }

        return null;
    }
}
