using Marten;

namespace TastyBeans.Profile.Application.ReadModels;

public class CustomerInfoRepository: ICustomerInfoRepository
{
    private readonly IDocumentSession _documentSession;

    public CustomerInfoRepository(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public async Task<CustomerInfo?> FindByIdAsync(Guid id)
    {
        return await _documentSession.Query<CustomerInfo>().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<CustomerInfo>> FindAllAsync()
    {
        return await _documentSession.Query<CustomerInfo>().OrderBy(x => x.LastName).ToListAsync();
    }
}