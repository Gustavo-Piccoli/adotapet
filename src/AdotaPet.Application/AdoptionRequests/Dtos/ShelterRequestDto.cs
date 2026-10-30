using AdotaPet.Domain.AdoptionRequests;

namespace AdotaPet.Application.AdoptionRequests.Dtos;

public sealed record ShelterRequestDto(Guid Id, Guid PetId, string PetName, string AdopterName,
    string AdopterEmail, string AdopterPhone, string AdopterCity, string AdopterState, string Message,
    AdoptionRequestStatus Status, DateTimeOffset CreatedAt, DateTimeOffset? DecidedAt);
