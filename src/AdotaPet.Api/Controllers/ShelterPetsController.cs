using AdotaPet.Api.Auth;
using AdotaPet.Application.Pets.Dtos;
using AdotaPet.Application.Pets.UseCases;
using AdotaPet.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdotaPet.Api.Controllers;

[ApiController]
[Authorize(Policy = Policies.Shelter)]
[Route("api/shelter/pets")]
public sealed class ShelterPetsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShelterPetDto>>> List(
        [FromServices] IListShelterPetsUseCase useCase, CancellationToken ct)
        => Ok(await useCase.ExecuteAsync(User.GetProfileId(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShelterPetDto>> Get(
        Guid id, [FromServices] IGetShelterPetUseCase useCase, CancellationToken ct)
        => Ok(await useCase.ExecuteAsync(User.GetProfileId(), id, ct));

    [HttpPost]
    public async Task<IActionResult> Create(
        PetInputDto input, [FromServices] ICreatePetUseCase useCase, CancellationToken ct)
    {
        var id = await useCase.ExecuteAsync(User.GetProfileId(), input, ct);
        return Created($"/api/shelter/pets/{id}", new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, PetInputDto input, [FromServices] IUpdatePetUseCase useCase, CancellationToken ct)
    {
        await useCase.ExecuteAsync(User.GetProfileId(), id, input, ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/photo")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadPhoto(
        Guid id, IFormFile? file, [FromServices] IUploadPetPhotoUseCase useCase, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            throw new DomainValidationException("Envie uma imagem no campo 'file'.");

        await using var stream = file.OpenReadStream();
        await useCase.ExecuteAsync(User.GetProfileId(), id, stream, file.Length, ct);
        return NoContent();
    }
}
