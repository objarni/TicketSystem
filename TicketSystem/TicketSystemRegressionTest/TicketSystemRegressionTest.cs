using System.Text.Json;
using ApprovalTests;
using ApprovalTests.Reporters;
using TicketManagementSystem;

namespace TicketSystemRegressionTest;

[UseReporter(typeof(DiffReporter))]
public class Tests
{
    [Test]
    public void CreateTicketTest()
    {
        var title = "title";
        var priority = Priority.High;
        string assignedTo = null;
        var description = "description";
        var created = default(DateTime);
        var isPayingCustomer = false;
        var user = new User();
        user.Username = "username";
        user.FirstName = "Olof";
        user.LastName = "Bjarnason";
        var ticket = TicketService.CreateTicketInner(
            title,
            priority,
            assignedTo,
            description,
            created,
            isPayingCustomer,
            user);
        var toVerify = JsonSerializer.Serialize(ticket,
            new JsonSerializerOptions { WriteIndented = true });

        Approvals.Verify(toVerify);
    }    [Test]

    public void CreateTicketTestWithException()
    {
        string title = null;
        var priority = Priority.High;
        string assignedTo = null;
        var description = "description";
        var created = default(DateTime);
        var isPayingCustomer = false;
        var user = new User();
        user.Username = "username";
        user.FirstName = "Olof";
        user.LastName = "Bjarnason";
        string toVerify = "";
        try
        {
            var ticket = TicketService.CreateTicketInner(
                title,
                priority,
                assignedTo,
                description,
                created,
                isPayingCustomer,
                user);
            toVerify = JsonSerializer.Serialize(ticket,
                new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception e)
        {
            toVerify = e.ToString();
        }

        Approvals.Verify(toVerify);
    }
}