using AdotaPet.Domain.AdoptionRequests;
using AdotaPet.Domain.Common;

namespace AdotaPet.Application.AdoptionRequests;

internal static class RequestLookup
{
    /// <summary>Loads a request whose pet belongs to the shelter; anything else is a 404.</summary>
    public static async Task<AdoptionRequest> GetOwnedAsync(IAdoptionRequestRepository requests,
        Guid shelterId, Guid requestId, CancellationToken ct)
    {
        var request = await requests.GetByIdAsync(requestId, ct);
        if (request is null) throw new NotFoundException("Pedido não encontrado.");
        var pet = request.Pet ?? throw new InvalidOperationException("Request loaded without its pet.");
        return pet.ShelterId == shelterId ? request : throw new NotFoundException("Pedido não encontrado.");
    }
}
