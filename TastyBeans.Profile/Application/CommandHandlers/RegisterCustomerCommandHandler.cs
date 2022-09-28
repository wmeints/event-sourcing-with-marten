using Marten;

using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Commands;

namespace TastyBeans.Profile.Application.CommandHandlers;

public class RegisterCustomerCommandHandler
{
    private readonly IDocumentSession _documentSession;

    public RegisterCustomerCommandHandler(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public async Task HandleAsync(RegisterCustomer cmd)
    {
        
    }
}