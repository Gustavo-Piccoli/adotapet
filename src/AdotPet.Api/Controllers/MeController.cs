using AdotPet.Api.Auth;
using AdotPet.Application.Accounts.Dtos;
using AdotPet.Application.Accounts.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdotPet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public sealed class MeController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MeDto>> Get([FromServices] IGetMeUseCase useCase, CancellationToken ct)
        => Ok(await useCase.ExecuteAsync(User.GetUserId(), User.GetRole(), ct));
}
