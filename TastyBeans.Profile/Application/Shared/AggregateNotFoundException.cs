namespace TastyBeans.Profile.Application.Shared;

public class AggregateNotFoundException: Exception
{
    public AggregateNotFoundException(string message): base(message)
    {
        
    }
}