using System.Text.Json;
using System.Text.RegularExpressions;
using ApprovalTests;
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
        var priority = Priority.High;
        string assignedTo = null;
        var description = "description";
        var created = DateTime.Parse("2023-05-31 12:00");
        var utcNow = DateTime.Parse("2023-05-31 15:00");
        var isPayingCustomer = false;
        var user = new User();
        user.Username = "username";
        user.FirstName = "Olof";
        user.LastName = "Bjarnason";
        var titles
            = new[] { null, "title" };
        CombinationApprovals.VerifyAllCombinations(
            (a1, a2, a3, a4, a5, a6, a7, a8) =>
                Scrubber(ToVerify(a1, a2, a3, a4, a5, a6, a7, a8)),
            titles,
            new[] { priority },
            new[] { assignedTo },
            new[] { description },
            new[] { created },
            new[] { isPayingCustomer },
            new[] { user },
            new[] { utcNow });
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