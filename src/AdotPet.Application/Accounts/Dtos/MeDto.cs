namespace AdotPet.Application.Accounts.Dtos;

public sealed record MeDto(string Role, Guid ProfileId, string Name, string Email);
