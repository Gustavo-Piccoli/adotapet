using AdotPet.Application.Common;
using AdotPet.Domain.AdoptionRequests;
using AdotPet.Domain.Common;

namespace AdotPet.Application.AdoptionRequests.UseCases;

public interface IRejectAdoptionRequestUseCase
{
    Task ExecuteAsync(Guid shelterId, Guid requestId, CancellationToken ct);
}

public sealed class RejectAdoptionRequestUseCase(IAdoptionRequestRepository requests,
    IUnitOfWork unitOfWork, IClock clock) : IRejectAdoptionRequestUseCase
{
    public async Task ExecuteAsync(Guid shelterId, Guid requestId, CancellationToken ct)
    {
        var request = await RequestLookup.GetOwnedAsync(requests, shelterId, requestId, ct);
        request.Reject(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
