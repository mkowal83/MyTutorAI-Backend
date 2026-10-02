using MyTutorAI.Application.Common;

namespace MyTutorAI.Infrastructure.Common;

/// <summary>
/// EN: <see cref="IClock"/> backed by the system clock.
/// PL: <see cref="IClock"/> oparty na zegarze systemowym.
/// </summary>
internal sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
