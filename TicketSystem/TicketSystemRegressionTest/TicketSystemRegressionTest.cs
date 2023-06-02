using System.Text.Json;
using System.Text.RegularExpressions;
using ApprovalTests.Combinations;
using ApprovalTests.Reporters;
using EmailService;
using TicketManagementSystem;

namespace TicketSystemRegressionTest;

[UseReporter(typeof(DiffReporter))]
public class Tests
{
    class SpyingEmailService : IEmailService
    {
        public string SentIncidentTitle = "", SentAssignedTo = "";

        public void SendEmailToAdministrator(string incidentTitle, string assignedTo)
        {
            SentIncidentTitle = incidentTitle;
            SentAssignedTo = assignedTo;
        }
    }
    [UseReporter(typeof(DiffReporter))]
    [Test]
    public void CreateTicketTest()
    {
        UserRepository.TestMode = true;
        var user = new User
        {
            FirstName = "Olof", LastName = "Bjarnason", Username = "objarni"
        };
        var titles
            = new[] { null, "title", "Crash" };
        var priorities = new[] { Priority.Low, Priority.Medium, Priority.High };
        var assignedTos = new[] { (string)null, "Bjarni" };
        var descriptions = new[] { null, "description" };
        var users = new[] { null, user };
        var createdTimes = new[] { DateTime.Parse("2023-05-31 12:00") };
        var utcNowTimes = new[] { DateTime.Parse("2023-05-31 15:00"), DateTime.Parse("2023-05-31 12:30") };
        CombinationApprovals.VerifyAllCombinations(
            (a1, a2, a3, a4, a5, a6, a7, a8) =>
                Scrubber(ToVerify(a1, a2, a3, a4, a5, a6, a7, a8)),
            titles,
            priorities,
            assignedTos,
            descriptions,
            createdTimes,
            new[] { true, false },
            users,
            utcNowTimes);
    }

    private string Scrubber(string toVerify)
    {
        return Regex.Replace(toVerify, ":line (\\d+)", "line: <LINE>",
            RegexOptions.Multiline);
    }

    private static string ToVerify(string? title, Priority priority, string? assignedTo, string description,
        DateTime created, bool isPayingCustomer, User user, DateTime utcNow)
    {
        var toVerify = "";
        try
        {
            var spyingEmailService = new SpyingEmailService();
            var ticket = TicketService.TicketInnerDeterministic(
                title,
                priority,
                assignedTo,
                description,
                created,
                isPayingCustomer,
                user,
                utcNow,
                spyingEmailService);
            toVerify = JsonSerializer.Serialize(ticket,
                new JsonSerializerOptions { WriteIndented = true });
            if (spyingEmailService.SentIncidentTitle != "")
            {
                toVerify += "\nSent incident title: " + spyingEmailService.SentIncidentTitle;
                toVerify += "\nSent assigned to: " + spyingEmailService.SentAssignedTo;
            }
        }
        catch (Exception e)
        {
            toVerify = e.ToString();
        }

        return toVerify;
    }
}