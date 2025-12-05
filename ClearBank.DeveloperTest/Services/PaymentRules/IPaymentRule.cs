using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services.PaymentRules
{
    public interface IPaymentRule
    {
        PaymentScheme PaymentScheme { get; }

        bool IsValid(Account account, MakePaymentRequest paymentRequest);
    }
}
