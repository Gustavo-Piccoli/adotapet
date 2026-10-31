using AdotPet.Api.Auth;
using AdotPet.Application.AdoptionRequests.Dtos;
using AdotPet.Application.AdoptionRequests.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdotPet.Api.Controllers;

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
