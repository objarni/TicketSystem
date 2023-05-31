using System.Text.Json;
using ApprovalTests.Reporters;
using TicketManagementSystem;

namespace TicketSystemRegressionTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [UseReporter(typeof(DiffReporter))]
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
            new JsonSerializerOptions() { WriteIndented = true });
        
        ApprovalTests.Approvals.Verify(toVerify);
    }
}