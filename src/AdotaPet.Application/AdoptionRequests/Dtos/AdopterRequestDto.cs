using AdotaPet.Domain.AdoptionRequests;

namespace AdotaPet.Application.AdoptionRequests.Dtos;

public sealed record AdopterRequestDto(Guid Id, Guid PetId, string PetName, string? PetPhotoUrl,
    string ShelterName, string Message, AdoptionRequestStatus Status, DateTimeOffset CreatedAt,
    DateTimeOffset? DecidedAt);
