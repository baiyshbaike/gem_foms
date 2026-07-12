namespace Domain.Sessions;

public enum SessionStatus
{
    Identified = 1,
    Started = 2,
    Paused = 3,
    Finished = 4,
    EndIdentified = 5,
    SentToPay = 6,
    Paid = 7,
    Archived = 8,

    IdentificationExpired = 20,
    EndIdentificationOverdue = 21,
    SendToPayOverdue = 22,

    Cancelled = 90
}