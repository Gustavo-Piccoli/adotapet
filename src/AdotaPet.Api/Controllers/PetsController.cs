using AdotaPet.Api.Auth;
using AdotaPet.Api.Contracts;
using AdotaPet.Application.AdoptionRequests.UseCases;
using AdotaPet.Application.Pets.Dtos;
using AdotaPet.Application.Pets.UseCases;
using AdotaPet.Domain.Common;
using AdotaPet.Domain.Pets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdotaPet.Api.Controllers;

[ApiController]
[Route("api/pets")]
public sealed class PetsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<PetSummaryDto>>> Search(
        [FromQuery] Species? species, [FromQuery] Sex? sex, [FromQuery] Size? size,
        [FromQuery] AgeGroup? ageGroup, [FromQuery] string? state, [FromQuery] string? city,
        [FromQuery] int? page, [FromQuery] int? pageSize,
        [FromServices] ISearchPetsUseCase useCase, CancellationToken ct)
        => Ok(await useCase.ExecuteAsync(
            new SearchPetsDto(species, sex, size, ageGroup, state, city, page, pageSize), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PetDetailsDto>> Get(
        Guid id, [FromServices] IGetPetDetailsUseCase useCase, CancellationToken ct)
        => Ok(await useCase.ExecuteAsync(id, ct));

    [HttpGet("{id:guid}/photo")]
    public async Task<IActionResult> GetPhoto(
        Guid id, [FromServices] IGetPetPhotoUseCase useCase, CancellationToken ct)
    {
        var photo = await useCase.ExecuteAsync(id, ct);
        Response.Headers.CacheControl = "public, max-age=3600";
        return File(photo.Content, photo.ContentType);
    }

    [HttpPost("{id:guid}/adoption-requests")]
    [Authorize(Policy = Policies.Adopter)]
    public async Task<IActionResult> CreateAdoptionRequest(
        Guid id, CreateAdoptionRequestBody body,
        [FromServices] ICreateAdoptionRequestUseCase useCase, CancellationToken ct)
    {
        var requestId = await useCase.ExecuteAsync(User.GetProfileId(), id, body.Message, ct);
        // No endpoint reads a single request, so there is no meaningful Location to send.
        return StatusCode(StatusCodes.Status201Created, new { id = requestId });
    }
}
