namespace AdotaPet.Application.Accounts.Dtos;

public sealed record AuthResultDto(string AccessToken, DateTimeOffset ExpiresAt, string Role,
    Guid ProfileId, string Name, string Email);
