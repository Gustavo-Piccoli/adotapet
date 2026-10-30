namespace AdotaPet.Domain.Common;

public sealed class NotFoundException(string message) : DomainException(message);
