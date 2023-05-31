using TicketManagementSystem;

namespace TicketSystemRegressionTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void CreateTicketTest()
    {
        var ticket = new TicketService().CreateTicket(
            null,
            Priority.High,
            null, 
            null, 
            default, 
            false);
        
        ApprovalTests.Approvals.Verify(ticket);
    }
}