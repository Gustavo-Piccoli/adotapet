using AdotaPet.Application.Accounts.Dtos;
using AdotaPet.Domain.Adopters;
using AdotaPet.Domain.Shelters;

namespace AdotaPet.Application.Accounts.UseCases;

public interface ILoginUseCase
{
    Task<AuthResultDto> ExecuteAsync(LoginDto dto, CancellationToken ct);
}

public sealed class LoginUseCase(IIdentityService identity, ITokenService tokens,
    IShelterRepository shelters, IAdopterRepository adopters) : ILoginUseCase
{
    public async Task<AuthResultDto> ExecuteAsync(LoginDto dto, CancellationToken ct)
    {
        var user = await identity.ValidateCredentialsAsync(dto.Email, dto.Password, ct)
            ?? throw new InvalidCredentialsException();

        (Guid Id, string Name, string Email) profile;
        if (user.Role == Roles.Shelter)
        {
            var shelter = await shelters.GetByUserIdAsync(user.UserId, ct) ?? throw new InvalidCredentialsException();
            profile = (shelter.Id, shelter.Name, shelter.Email);
        }
        else if (user.Role == Roles.Adopter)
        {
            var adopter = await adopters.GetByUserIdAsync(user.UserId, ct) ?? throw new InvalidCredentialsException();
            profile = (adopter.Id, adopter.Name, adopter.Email);
        }
        else
        {
            throw new InvalidCredentialsException();
        }

        var jwt = tokens.CreateToken(user.UserId, profile.Email, user.Role, profile.Id, profile.Name);
        return new AuthResultDto(jwt.AccessToken, jwt.ExpiresAt, user.Role, profile.Id, profile.Name, profile.Email);
    }
}
