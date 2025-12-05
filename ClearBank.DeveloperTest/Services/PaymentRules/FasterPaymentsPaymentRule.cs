using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services.PaymentRules
{
    public class FasterPaymentsPaymentRule : IPaymentRule
    {
        public PaymentScheme PaymentScheme => PaymentScheme.FasterPayments;

        public bool IsValid(Account account, MakePaymentRequest paymentRequest)
        {
            return account != null && account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments) && account.Balance >= paymentRequest.Amount;
        }
    }
}
