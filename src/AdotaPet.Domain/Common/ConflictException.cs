namespace AdotaPet.Domain.Common;

public sealed class ConflictException(string message) : DomainException(message);
