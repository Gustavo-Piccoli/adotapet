using AdotaPet.Application.Accounts.Dtos;
using AdotaPet.Application.Accounts.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace AdotaPet.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("register/shelter")]
    public async Task<ActionResult<AuthResultDto>> RegisterShelter(
        RegisterShelterDto dto, [FromServices] IRegisterShelterUseCase useCase, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await useCase.ExecuteAsync(dto, ct));

    [HttpPost("register/adopter")]
    public async Task<ActionResult<AuthResultDto>> RegisterAdopter(
        RegisterAdopterDto dto, [FromServices] IRegisterAdopterUseCase useCase, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await useCase.ExecuteAsync(dto, ct));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login(
        LoginDto dto, [FromServices] ILoginUseCase useCase, CancellationToken ct)
        => Ok(await useCase.ExecuteAsync(dto, ct));
}
