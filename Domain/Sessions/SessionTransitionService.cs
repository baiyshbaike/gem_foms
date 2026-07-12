namespace Domain.Sessions;

public static class SessionTransitionService
{
    public static bool CanMove(SessionStatus from, SessionStatus to, bool hasOverride = false)
    {
        if (hasOverride && to == SessionStatus.Archived)
        {
            return true;
        }

        return (from, to) switch
        {
            (SessionStatus.Identified, SessionStatus.Started) => true,
            (SessionStatus.Identified, SessionStatus.IdentificationExpired) => true,
            (SessionStatus.Identified, SessionStatus.Cancelled) => true,

            (SessionStatus.Started, SessionStatus.Paused) => true,
            (SessionStatus.Started, SessionStatus.Finished) => true,
            (SessionStatus.Started, SessionStatus.Cancelled) => true,

            (SessionStatus.Paused, SessionStatus.Started) => true,
            (SessionStatus.Paused, SessionStatus.Cancelled) => true,

            (SessionStatus.Finished, SessionStatus.EndIdentified) => true,
            (SessionStatus.Finished, SessionStatus.EndIdentificationOverdue) => true,

            (SessionStatus.EndIdentificationOverdue, SessionStatus.EndIdentified) => hasOverride,

            (SessionStatus.EndIdentified, SessionStatus.SentToPay) => true,
            (SessionStatus.EndIdentified, SessionStatus.SendToPayOverdue) => true,

            (SessionStatus.SendToPayOverdue, SessionStatus.SentToPay) => hasOverride,

            (SessionStatus.SentToPay, SessionStatus.Paid) => true,
            (SessionStatus.Paid, SessionStatus.Archived) => true,

            _ => false
        };
    }
}