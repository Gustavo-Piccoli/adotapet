using AdotPet.Application.Accounts.Dtos;
using AdotPet.Domain.Adopters;
using AdotPet.Domain.Common;
using AdotPet.Domain.Shelters;

namespace AdotPet.Application.Accounts.UseCases;

public interface IGetMeUseCase
{
    Task<MeDto> ExecuteAsync(string userId, string role, CancellationToken ct);
}

public sealed class GetMeUseCase(IShelterRepository shelters, IAdopterRepository adopters) : IGetMeUseCase
{
    public async Task<MeDto> ExecuteAsync(string userId, string role, CancellationToken ct)
    {
        if (role == Roles.Shelter)
        {
            var shelter = await shelters.GetByUserIdAsync(userId, ct) ?? throw ProfileNotFound();
            return new MeDto(Roles.Shelter, shelter.Id, shelter.Name, shelter.Email);
        }

        if (role == Roles.Adopter)
        {
            var adopter = await adopters.GetByUserIdAsync(userId, ct) ?? throw ProfileNotFound();
            return new MeDto(Roles.Adopter, adopter.Id, adopter.Name, adopter.Email);
        }

        throw ProfileNotFound();
    }

    private static NotFoundException ProfileNotFound() => new("Perfil não encontrado.");
}
