namespace TicketManagementSystem;

public interface ITimeService
{
    DateTime GetCurrentTime();
}

public class UTCTimeService : ITimeService
{
    public DateTime GetCurrentTime()
    {
        return DateTime.UtcNow;
    }
}