using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services.PaymentRules
{
    public class ChapsPaymentRule : IPaymentRule
    {
        public PaymentScheme PaymentScheme => PaymentScheme.Chaps;

        public bool IsValid(Account account, MakePaymentRequest paymentRequest)
        {
            return account != null && account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Chaps) && account.Status == AccountStatus.Live;
        }
    }
}
