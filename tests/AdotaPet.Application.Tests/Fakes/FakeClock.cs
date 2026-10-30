using AdotaPet.Application.Common;

namespace AdotaPet.Application.Tests.Fakes;

public sealed class FakeClock(DateTimeOffset? now = null) : IClock
{
    public DateTimeOffset UtcNow { get; set; } = now ?? new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);
}
