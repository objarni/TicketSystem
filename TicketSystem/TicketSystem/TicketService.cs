using System.Text.Json;
using EmailService;

namespace TicketManagementSystem;

public class TicketService
{
    public int CreateTicket(string t, Priority priority, string assignedTo, string desc, DateTime d, bool isPayingCustomer)
    {
        if (t == null || desc == null || t == "" || desc == "")
        {
            throw new InvalidTicketException("Title or description were null");
        }

        User user = null;
        using (var ur = new UserRepository())
        {
            if (assignedTo != null)
            {
                user = ur.GetUser(assignedTo);
            }
        }

        if (user == null)
        {
            throw new UnknownUserException("User " + assignedTo + " not found");
        }

        var priorityRaised = false;
        if (d < DateTime.UtcNow - TimeSpan.FromHours(1))
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

        if ((t.Contains("Crash") || t.Contains("Important") || t.Contains("Failure")) && !priorityRaised)
        {
            if (priority == Priority.Low)
            {
                priority = Priority.Medium;
            }
            else if (priority == Priority.Medium)
            {
                priority = Priority.High;
            }
        }

        if (priority == Priority.High)
        {
            var emailService = new EmailServiceProxy();
            emailService.SendEmailToAdministrator(t, assignedTo);
        }

        double price = 0;
        User accountManager = null;
        if (isPayingCustomer)
        {
            accountManager = new UserRepository().GetAccountManager();
            if (priority == Priority.High)
            {
                price = 100;
            }
            else
            {
                price = 50;
            }
        }

        var ticket = new Ticket()
        {
            Title = t,
            AssignedUser = user,
            Priority = priority,
            Description = desc,
            Created = d,
            PriceDollars = price,
            AccountManager = accountManager
        };

        var id = TicketRepository.CreateTicket(ticket);

        return id;
    }

    public void AssignTicket(int id, string username)
    {
        User user = null;
        using (var ur = new UserRepository())
        {
            if (username != null)
            {
                user = ur.GetUser(username);
            }
        }

        if (user == null)
        {
            throw new UnknownUserException("User not found");
        }

        var ticket = TicketRepository.GetTicket(id);

        if (ticket == null)
        {
            throw new ApplicationException("No ticket found for id " + id);
        }

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