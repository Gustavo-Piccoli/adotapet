namespace AdotaPet.Application.Accounts.Dtos;

public sealed record RegisterShelterDto(string Email, string Password, string Name, string Cnpj,
    string City, string State, string Phone, string Description);
