using System.Text.Json;
using System.Text.RegularExpressions;
using ApprovalTests.Combinations;
using ApprovalTests.Reporters;
using TicketManagementSystem;

namespace TicketSystemRegressionTest;

[UseReporter(typeof(DiffReporter))]
public class Tests
{
    [UseReporter(typeof(DiffReporter))]
    [Test]
    public void CreateTicketTest()
    {
        var user = new User
        {
            FirstName = "Olof", LastName = "Bjarnason", Username = "objarni"
        };
        var titles
            = new[] { null, "title", "Crash" };
        var priorities = new[] { Priority.Medium, Priority.High };
        var assignedTos = new[] { (string)null, "Bjarni" };
        var descriptions = new[] { null, "description" };
        var users = new[] { user };
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
            var ticket = TicketService.TicketInnerDeterministic(
                title,
                priority,
                assignedTo,
                description,
                created,
                isPayingCustomer,
                user,
                utcNow);
            toVerify = JsonSerializer.Serialize(ticket,
                new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception e)
        {
            toVerify = e.ToString();
        }

        return toVerify;
    }
}