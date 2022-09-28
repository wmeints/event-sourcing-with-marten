namespace TastyBeans.Profile.Application.ReadModels;

public record CustomerInfo(Guid Id, string FirstName, string LastName,SubscriptionStatus Status)
{
    
}