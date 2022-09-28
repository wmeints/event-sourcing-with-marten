using Marten;

using TastyBeans.Profile.Application.Shared;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Commands;

namespace TastyBeans.Profile.Application.CommandHandlers;

public class UnsubscribeCustomerCommandHandler
{
    private readonly IDocumentSession _documentSession;

    public UnsubscribeCustomerCommandHandler(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public async Task HandleAsync(UnsubscribeCustomer cmd)
    {
        var customer = await _documentSession.Events.AggregateStreamAsync<Customer>(cmd.CustomerId);

        if (customer == null)
        {
            throw new AggregateNotFoundException($"Couldn't find customer with ID {cmd.CustomerId}");
        }
        
        customer.Unsubscribe(cmd);

        _documentSession.Events.Append(customer.Id, customer.PendingDomainEvents);
        await _documentSession.SaveChangesAsync();
    }
}