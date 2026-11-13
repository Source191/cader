using Source191.Cader.Application.Actions;
using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Actions;

public sealed class OpenSolutionHandlerShould
{
    [Fact]
    public async Task OpenTheSolutionWithItsDefaultApplication()
    {
        FakeDesktop desktop = new();
        OpenSolutionHandler handler = new(desktop);

        Outcome outcome = await handler.HandleAsync(new OpenSolution(@"C:\Users\me\source\repos\Northwind.Orders\Northwind.Orders.slnx"), CancellationToken.None);

        desktop.Opened.ShouldBe([@"C:\Users\me\source\repos\Northwind.Orders\Northwind.Orders.slnx"]);
        outcome.ShouldBe(new OpenSolutionOutcome.Opened("Northwind.Orders.slnx"));
    }

    [Fact]
    public async Task ReportATypeNoApplicationOpens()
    {
        FakeDesktop desktop = new() { OpenFailure = new NoApplicationException(".slnx") };
        OpenSolutionHandler handler = new(desktop);

        Outcome outcome = await handler.HandleAsync(new OpenSolution(@"C:\Users\me\source\repos\Northwind.Orders\Northwind.Orders.slnx"), CancellationToken.None);

        outcome.ShouldBe(new OpenSolutionOutcome.NoApplication(".slnx"));
    }
}