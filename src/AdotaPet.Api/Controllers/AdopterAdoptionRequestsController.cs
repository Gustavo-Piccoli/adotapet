using AdotaPet.Api.Auth;
using AdotaPet.Application.AdoptionRequests.Dtos;
using AdotaPet.Application.AdoptionRequests.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdotaPet.Api.Controllers;

[ApiController]
[Authorize(Policy = Policies.Adopter)]
[Route("api/adopter/adoption-requests")]
public sealed class AdopterAdoptionRequestsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdopterRequestDto>>> List(
        [FromServices] IListAdopterRequestsUseCase useCase, CancellationToken ct)
        => Ok(await useCase.ExecuteAsync(User.GetProfileId(), ct));
}
