namespace AdotaPet.Domain.Common;

/// <summary>
/// A 409. <see cref="Code"/> is a stable, machine-readable reason the API exposes as the `code` extension of
/// the ProblemDetails, so the client can tell apart conflicts that share the same status. See <see cref="ConflictCodes"/>.
/// </summary>
public sealed class ConflictException(string message, string code) : DomainException(message)
{
    public string Code { get; } = code;
}

/// <summary>Values of the ProblemDetails `code` extension on 409 responses.</summary>
public static class ConflictCodes
{
    /// <summary>The adopter already has a pending request for this pet.</summary>
    public const string DuplicatePendingRequest = "duplicate_pending_request";

    /// <summary>The pet is not available for adoption (already adopted).</summary>
    public const string PetUnavailable = "pet_unavailable";

    /// <summary>Approving a request whose pet was already adopted by another request.</summary>
    public const string PetAlreadyAdopted = "pet_already_adopted";

    /// <summary>The request was already approved or rejected.</summary>
    public const string RequestAlreadyDecided = "request_already_decided";

    /// <summary>An account with this e-mail already exists.</summary>
    public const string EmailTaken = "email_taken";

    /// <summary>A shelter with this CNPJ already exists.</summary>
    public const string CnpjTaken = "cnpj_taken";

    /// <summary>Optimistic concurrency: someone else changed the same row first.</summary>
    public const string ConcurrencyConflict = "concurrency_conflict";

    /// <summary>Any other unique-constraint violation.</summary>
    public const string DuplicateRecord = "duplicate_record";
}
