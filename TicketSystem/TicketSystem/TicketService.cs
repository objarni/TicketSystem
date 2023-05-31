using System.Text.Json;
using EmailService;

namespace TicketManagementSystem;

public class TicketService
{
    public int CreateTicket(string ticketStringlyType, Priority priority, string assignedTo, string description,
        DateTime created, bool isPayingCustomer)
    {
        if (ticketStringlyType == null || description == null || ticketStringlyType == "" || description == "")
            throw new InvalidTicketException("Title or description were null");

        User user = null;
        using (var ur = new UserRepository())
        {
            if (assignedTo != null) user = ur.GetUser(assignedTo);
        }

        if (user == null) throw new UnknownUserException("User " + assignedTo + " not found");

        var priorityRaised = false;
        if (created < DateTime.UtcNow - TimeSpan.FromHours(1))
        {
            if (priority == Priority.Low)
            {
                priority = Priority.Medium;
                priorityRaised = true;
            }
            else if (priority == Priority.Medium)
            {
                priority = Priority.High;
                priorityRaised = true;
            }
        }

        if ((ticketStringlyType.Contains("Crash") || ticketStringlyType.Contains("Important") ||
             ticketStringlyType.Contains("Failure")) && !priorityRaised)
        {
            if (priority == Priority.Low)
                priority = Priority.Medium;
            else if (priority == Priority.Medium) priority = Priority.High;
        }

        if (priority == Priority.High)
        {
            var emailService = new EmailServiceProxy();
            emailService.SendEmailToAdministrator(ticketStringlyType, assignedTo);
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
            Title = ticketStringlyType,
            AssignedUser = user,
            Priority = priority,
            Description = description,
            Created = created,
            PriceDollars = price,
            AccountManager = accountManager
        };

        return TicketRepository.CreateTicket(ticket);
    }

    public void AssignTicket(int id, string username)
    {
        User user = null;
        using (var userRepository = new UserRepository())
        {
            if (username != null) user = userRepository.GetUser(username);
        }

        if (user == null) throw new UnknownUserException("User not found");

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