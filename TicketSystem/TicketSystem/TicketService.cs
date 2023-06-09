using System.Diagnostics.CodeAnalysis;
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
        if (title == null || description == null || title == "" || description == "")
            throw new InvalidTicketException("Title or description were null");

        User user = FindUserOrThrow(assignedTo);

        if (utcNow == null)
            utcNow = DateTime.UtcNow;
        
        priority = CalculatePriority(title, priority, created, utcNow.Value);

        if (priority == Priority.High)
        {
            if (emailService == null)
                emailService = new EmailServiceProxy();
            emailService.SendEmailToAdministrator(title, assignedTo);
        }

        double price = 0;
        User accountManager = null;
        if (isPayingCustomer)
        {
            accountManager = new UserRepository().GetAccountManager();
            price = priority == Priority.High ? 100 : 50;
        }

        var ticket = new Ticket
        {
            Title = title,
            AssignedUser = user,
            Priority = priority,
            Description = description,
            Created = created,
            PriceDollars = price,
            AccountManager = accountManager
        };
        return ticket;
    }

    private static Priority CalculatePriority(string title, Priority priority, DateTime created,
        DateTime utcNow)
    {
        if (created < utcNow - TimeSpan.FromHours(1))
        {
            switch (priority)
            {
                case Priority.Low:
                    return Priority.Medium;
                case Priority.Medium:
                    return Priority.High;
            }
        }

        if ((!title.Contains("Crash") && !title.Contains("Important") &&
             !title.Contains("Failure"))) return priority;
        if (priority == Priority.Low)
            priority = Priority.Medium;
        else if (priority == Priority.Medium) priority = Priority.High;

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
        User user = FindUserOrThrow(username);

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