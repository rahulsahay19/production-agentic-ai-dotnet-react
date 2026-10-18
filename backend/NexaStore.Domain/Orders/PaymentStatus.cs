namespace NexaStore.Domain.Orders;

public enum PaymentStatus
{
    Pending = 1,
    Authorized = 2,
    Paid = 3,
    Refunded = 4,
    Failed = 5
}
