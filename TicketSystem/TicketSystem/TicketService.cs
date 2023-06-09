using System.Text.Json;
using EmailService;

namespace TicketManagementSystem;

public class TicketService
{
    public int CreateTicket(string title, Priority priority, string assignedTo, string description,
        DateTime created, bool isPayingCustomer)
    {
        var ticket = TicketInnerDeterministic(title, priority, assignedTo, description, created, isPayingCustomer);
        return TicketRepository.CreateTicket(ticket);
    }

    public static Ticket TicketInnerDeterministic(
        string title,
        Priority priority,
        string assignedTo,
        string description,
        DateTime created,
        bool isPayingCustomer,
        DateTime? utcNow = null,
        IEmailService emailService = null)
    {
        var ticket = MakeValidTicket(title, priority, assignedTo, description, created, isPayingCustomer, utcNow);

        if (ticket.Priority == Priority.High)
        {
            if (emailService == null)
                emailService = new EmailServiceProxy();
            emailService.SendEmailToAdministrator(title, assignedTo);
        }

        return ticket;
    }

    private static Ticket MakeValidTicket(string title, Priority priority, string assignedTo, string description,
        DateTime created, bool isPayingCustomer, DateTime? utcNow)
    {
        if (utcNow == null)
            utcNow = DateTime.UtcNow;

        if (title == null || description == null || title == "" || description == "")
            throw new InvalidTicketException("Title or description were null");

        var assignedUser = FindUserOrThrow(assignedTo);

        var finalPriority = CalculatePriority(title, priority, created, utcNow.Value);

        var ticket = new Ticket
        {
            Title = title,
            AssignedUser = assignedUser,
            Priority = finalPriority,
            Description = description,
            Created = created,
            PriceDollars = CalculatePrice(finalPriority, isPayingCustomer),
            AccountManager = MaybeFindAccountManager(isPayingCustomer)
        };
        return ticket;
    }

    private static User? MaybeFindAccountManager(bool isPayingCustomer)
    {
        User accountManager = null;
        if (isPayingCustomer) accountManager = new UserRepository().GetAccountManager();
        return accountManager;
    }

    private static double CalculatePrice(Priority priority, bool isPayingCustomer)
    {
        double price = 0;
        if (isPayingCustomer) price = priority == Priority.High ? 100 : 50;

        return price;
    }

    private static Priority CalculatePriority(string title, Priority priority, DateTime created,
        DateTime utcNow)
    {
        var urgent = created < utcNow - TimeSpan.FromHours(1);
        var important = title.Contains("Crash") || title.Contains("Important") ||
                        title.Contains("Failure");
        if (urgent || important)
            return RaisePriority(priority);

        return priority;
    }

    private static Priority RaisePriority(Priority priority)
    {
        switch (priority)
        {
            case Priority.Low:
                priority = Priority.Medium;
                break;
            case Priority.Medium:
                priority = Priority.High;
                break;
        }

        return priority;
    }

    private static User FindUserOrThrow(string? assignedTo)
    {
        User user = null;
        using (var ur = new UserRepository())
        {
            if (assignedTo != null) user = ur.GetUser(assignedTo);
        }

        if (user == null) throw new UnknownUserException("User " + assignedTo + " not found");
        return user;
    }

    public void AssignTicket(int id, string username)
    {
        var user = FindUserOrThrow(username);
        var ticket = TicketRepository.GetTicket(id);
        if (ticket == null) throw new ApplicationException("No ticket found for id " + id);
        ticket.AssignedUser = user;
        TicketRepository.UpdateTicket(ticket);
    }

    private void WriteTicketToFile(Ticket ticket)
    {
        var ticketJson = JsonSerializer.Serialize(ticket);
        File.WriteAllText(Path.Combine(Path.GetTempPath(), $"ticket_{ticket.Id}.json"), ticketJson);
    }
}

public enum Priority
{
    High,
    Medium,
    Low
}