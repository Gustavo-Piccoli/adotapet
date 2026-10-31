using AdotPet.Application.Accounts.Dtos;
using AdotPet.Application.Common;
using AdotPet.Domain.Common;
using AdotPet.Domain.Shelters;

namespace AdotPet.Application.Accounts.UseCases;

public interface IRegisterShelterUseCase
{
    Task<AuthResultDto> ExecuteAsync(RegisterShelterDto dto, CancellationToken ct);
}

public sealed class RegisterShelterUseCase(IIdentityService identity, ITokenService tokens,
    IShelterRepository shelters, IUnitOfWork unitOfWork, IClock clock) : IRegisterShelterUseCase
{
    public async Task<AuthResultDto> ExecuteAsync(RegisterShelterDto dto, CancellationToken ct)
    {
        var now = clock.UtcNow;

        // Build once up front so invalid input never produces an Identity user...
        var cnpj = Shelter.Create(dto.Name, dto.Cnpj, dto.City, dto.State, dto.Phone, dto.Email,
            dto.Description, now).Cnpj;

        if (await shelters.CnpjExistsAsync(cnpj, ct))
            throw new ConflictException("Já existe uma ONG com este CNPJ.", ConflictCodes.CnpjTaken);

        // ...but create the entity inside the transaction: the execution strategy may re-run the action, and
        // a reused entity would already have a user assigned.
        var (userId, shelter) = await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var shelter = Shelter.Create(dto.Name, dto.Cnpj, dto.City, dto.State, dto.Phone, dto.Email,
                dto.Description, now);
            var id = await identity.CreateUserAsync(dto.Email, dto.Password, Roles.Shelter, token);
            shelter.AssignUser(id);
            shelters.Add(shelter);
            await unitOfWork.SaveChangesAsync(token);
            return (id, shelter);
        }, ct);

        var jwt = tokens.CreateToken(userId, shelter.Email, Roles.Shelter, shelter.Id, shelter.Name);
        return new AuthResultDto(jwt.AccessToken, jwt.ExpiresAt, Roles.Shelter, shelter.Id, shelter.Name, shelter.Email);
    }
}
