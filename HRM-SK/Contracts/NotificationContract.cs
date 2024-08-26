namespace HRM_SK.Contracts
{
    public class NotificationContract
    {
    }
    public record NotificationRecord<TValue>(TValue Data, string NotifiableType, Guid NotifiableId) where TValue : class;

}
