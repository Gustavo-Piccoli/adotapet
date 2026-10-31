namespace AdotPet.Application.Accounts;

public sealed record IdentityUserInfo(string UserId, string Email, string Role);

public interface IIdentityService
{
    /// <summary>Creates an account and returns its user id. Duplicate e-mail throws ConflictException; weak password throws DomainValidationException.</summary>
    Task<string> CreateUserAsync(string email, string password, string role, CancellationToken ct);

    Task<IdentityUserInfo?> ValidateCredentialsAsync(string email, string password, CancellationToken ct);
}
