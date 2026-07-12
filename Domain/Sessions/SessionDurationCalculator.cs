namespace Domain.Sessions;

public static class SessionDurationCalculator
{
    public static TimeSpan CalculatePauseDuration(
        DateTimeOffset now,
        IReadOnlyCollection<HdSessionPause> pauses)
    {
        var totalMinutes = pauses.Sum(p =>
        {
            var pauseEnd = p.EndedAt ?? now;
            return Math.Max(0, (pauseEnd - p.StartedAt).TotalMinutes);
        });

        return TimeSpan.FromMinutes(totalMinutes);
    }

    public static TimeSpan CalculateActiveDuration(
        DateTimeOffset startedAt,
        DateTimeOffset now,
        IReadOnlyCollection<HdSessionPause> pauses)
    {
        var total = now - startedAt;
        var pauseDuration = CalculatePauseDuration(now, pauses);
        var active = total - pauseDuration;

        return active < TimeSpan.Zero ? TimeSpan.Zero : active;
    }
}