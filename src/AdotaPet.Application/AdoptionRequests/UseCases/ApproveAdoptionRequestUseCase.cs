using AdotaPet.Application.Common;
using AdotaPet.Domain.AdoptionRequests;
using AdotaPet.Domain.Common;

namespace AdotaPet.Application.AdoptionRequests.UseCases;

public interface IApproveAdoptionRequestUseCase
{
    Task ExecuteAsync(Guid shelterId, Guid requestId, CancellationToken ct);
}

public sealed class ApproveAdoptionRequestUseCase(IAdoptionRequestRepository requests,
    IUnitOfWork unitOfWork, IClock clock) : IApproveAdoptionRequestUseCase
{
    public async Task ExecuteAsync(Guid shelterId, Guid requestId, CancellationToken ct)
    {
        await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var request = await RequestLookup.GetOwnedAsync(requests, shelterId, requestId, token);
            var pet = request.Pet!;
            var now = clock.UtcNow;

            request.Approve(now);
            pet.MarkAdopted(now);

            foreach (var other in await requests.ListPendingByPetAsync(pet.Id, token))
            {
                if (other.Id != request.Id) other.Reject(now);
            }

            await unitOfWork.SaveChangesAsync(token);
            return true;
        }, ct);
    }
}
