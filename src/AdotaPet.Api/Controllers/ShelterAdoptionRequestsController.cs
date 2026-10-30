using AdotaPet.Api.Auth;
using AdotaPet.Application.AdoptionRequests.Dtos;
using AdotaPet.Application.AdoptionRequests.UseCases;
using AdotaPet.Domain.AdoptionRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdotaPet.Api.Controllers;

[ApiController]
[Authorize(Policy = Policies.Shelter)]
[Route("api/shelter/adoption-requests")]
public sealed class ShelterAdoptionRequestsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShelterRequestDto>>> List(
        [FromQuery] AdoptionRequestStatus? status,
        [FromServices] IListShelterRequestsUseCase useCase, CancellationToken ct)
        => Ok(await useCase.ExecuteAsync(User.GetProfileId(), status, ct));

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid id, [FromServices] IApproveAdoptionRequestUseCase useCase, CancellationToken ct)
    {
        await useCase.ExecuteAsync(User.GetProfileId(), id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid id, [FromServices] IRejectAdoptionRequestUseCase useCase, CancellationToken ct)
    {
        await useCase.ExecuteAsync(User.GetProfileId(), id, ct);
        return NoContent();
    }
}
