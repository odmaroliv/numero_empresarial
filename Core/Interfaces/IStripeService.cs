namespace NumeroEmpresarial.Core.Interfaces
{
    public interface IStripeService
    {
        Task<string> CreateCheckoutSessionAsync(Guid userId, decimal amount, string description);
        Task<bool> ProcessPaymentSuccessAsync(string sessionId);
        Task<string> CreateSubscriptionCheckoutAsync(Guid userId, Guid planId);
        Task<bool> ProcessSubscriptionSuccessAsync(string sessionId);
        Task<bool> CancelSubscriptionAsync(string subscriptionId);
        Task HandleWebhookAsync(string json, string signature);
    }
}