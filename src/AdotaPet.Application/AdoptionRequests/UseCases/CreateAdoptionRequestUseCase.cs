using AdotaPet.Application.Common;
using AdotaPet.Application.Pets;
using AdotaPet.Domain.AdoptionRequests;
using AdotaPet.Domain.Common;
using AdotaPet.Domain.Pets;

namespace AdotaPet.Application.AdoptionRequests.UseCases;

public interface ICreateAdoptionRequestUseCase
{
    Task<Guid> ExecuteAsync(Guid adopterId, Guid petId, string message, CancellationToken ct);
}

public sealed class CreateAdoptionRequestUseCase(IPetRepository pets, IAdoptionRequestRepository requests,
    IUnitOfWork unitOfWork, IClock clock) : ICreateAdoptionRequestUseCase
{
    public async Task<Guid> ExecuteAsync(Guid adopterId, Guid petId, string message, CancellationToken ct)
    {
        var pet = await pets.GetByIdAsync(petId, ct) ?? throw PetMapping.PetNotFound();

        if (await requests.HasPendingAsync(adopterId, petId, ct))
            throw new ConflictException("Você já tem um pedido em análise para este pet.", ConflictCodes.DuplicatePendingRequest);

        var request = AdoptionRequest.Create(pet, adopterId, message, clock.UtcNow);
        requests.Add(request);
        await unitOfWork.SaveChangesAsync(ct);
        return request.Id;
    }
}
