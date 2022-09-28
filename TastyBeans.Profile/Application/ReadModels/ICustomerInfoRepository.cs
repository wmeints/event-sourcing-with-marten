namespace TastyBeans.Profile.Application.ReadModels;

public interface ICustomerInfoRepository
{
    Task<CustomerInfo?> FindByIdAsync(Guid id);
    Task<IEnumerable<CustomerInfo>> FindAllAsync();
}