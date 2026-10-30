using AdotaPet.Application.Accounts.Dtos;
using AdotaPet.Application.Common;
using AdotaPet.Domain.Adopters;
using AdotaPet.Domain.Common;

namespace AdotaPet.Application.Accounts.UseCases;

public interface IRegisterAdopterUseCase
{
    Task<AuthResultDto> ExecuteAsync(RegisterAdopterDto dto, CancellationToken ct);
}

public sealed class RegisterAdopterUseCase(IIdentityService identity, ITokenService tokens,
    IAdopterRepository adopters, IUnitOfWork unitOfWork, IClock clock) : IRegisterAdopterUseCase
{
    public async Task<AuthResultDto> ExecuteAsync(RegisterAdopterDto dto, CancellationToken ct)
    {
        // Create validates first, so invalid input never produces an Identity user.
        var adopter = Adopter.Create(dto.Name, dto.Email, dto.Phone, dto.City, dto.State, clock.UtcNow);

        var userId = await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var id = await identity.CreateUserAsync(dto.Email, dto.Password, Roles.Adopter, token);
            adopter.AssignUser(id);
            adopters.Add(adopter);
            await unitOfWork.SaveChangesAsync(token);
            return id;
        }, ct);

        var jwt = tokens.CreateToken(userId, adopter.Email, Roles.Adopter, adopter.Id, adopter.Name);
        return new AuthResultDto(jwt.AccessToken, jwt.ExpiresAt, Roles.Adopter, adopter.Id, adopter.Name, adopter.Email);
    }
}
