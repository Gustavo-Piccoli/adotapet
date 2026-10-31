using AdotaPet.Api.Auth;
using AdotaPet.Application.Accounts.Dtos;
using AdotaPet.Application.Accounts.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdotaPet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public sealed class MeController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MeDto>> Get([FromServices] IGetMeUseCase useCase, CancellationToken ct)
        => Ok(await useCase.ExecuteAsync(User.GetUserId(), User.GetRole(), ct));
}
