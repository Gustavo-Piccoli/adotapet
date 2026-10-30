namespace AdotaPet.Application.Accounts.Dtos;

public sealed record RegisterAdopterDto(string Email, string Password, string Name, string Phone,
    string City, string State);
