using AdotaPet.Application.Accounts.Dtos;
using AdotaPet.Application.Common;
using AdotaPet.Domain.Common;
using AdotaPet.Domain.Shelters;

namespace AdotaPet.Application.Accounts.UseCases;

public interface IRegisterShelterUseCase
{
    Task<AuthResultDto> ExecuteAsync(RegisterShelterDto dto, CancellationToken ct);
}

public sealed class RegisterShelterUseCase(IIdentityService identity, ITokenService tokens,
    IShelterRepository shelters, IUnitOfWork unitOfWork, IClock clock) : IRegisterShelterUseCase
{
    public async Task<AuthResultDto> ExecuteAsync(RegisterShelterDto dto, CancellationToken ct)
    {
        // Create validates first, so invalid input never produces an Identity user.
        var shelter = Shelter.Create(dto.Name, dto.Cnpj, dto.City, dto.State, dto.Phone, dto.Email,
            dto.Description, clock.UtcNow);

        if (await shelters.CnpjExistsAsync(shelter.Cnpj, ct))
            throw new ConflictException("Já existe uma ONG com este CNPJ.");

        var userId = await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var id = await identity.CreateUserAsync(dto.Email, dto.Password, Roles.Shelter, token);
            shelter.AssignUser(id);
            shelters.Add(shelter);
            await unitOfWork.SaveChangesAsync(token);
            return id;
        }, ct);

        var jwt = tokens.CreateToken(userId, shelter.Email, Roles.Shelter, shelter.Id, shelter.Name);
        return new AuthResultDto(jwt.AccessToken, jwt.ExpiresAt, Roles.Shelter, shelter.Id, shelter.Name, shelter.Email);
    }
}
