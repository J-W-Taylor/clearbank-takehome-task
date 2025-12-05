using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services.PaymentRules
{
    public class BacsPaymentRule : IPaymentRule
    {
        public PaymentScheme PaymentScheme => PaymentScheme.Bacs;

        public bool IsValid(Account account, MakePaymentRequest paymentRequest)
        {
            return account != null && account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Bacs);
        }
    }
}
