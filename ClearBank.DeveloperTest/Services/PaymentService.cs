using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services.PaymentRules;
using ClearBank.DeveloperTest.Types;
using System.Collections.Generic;

namespace ClearBank.DeveloperTest.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAccountDataStore _primary;
        private readonly IAccountDataStore _backup;

        private readonly Dictionary<PaymentScheme, IPaymentRule> _paymentRules = new();

        public PaymentService(IDataStoreFactory provider, IEnumerable<IPaymentRule> paymentRules)
        {
            _primary = provider.Primary;
            _backup = provider.Backup;

            foreach (var paymentRule in paymentRules)
            {
                _paymentRules.Add(paymentRule.PaymentScheme, paymentRule);
            }
        }

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            Account account;

            try
            {
                // Can be set up to return 'null' if no account can be found
                account = _primary.GetAccount(request.DebtorAccountNumber);
            }
            catch
            {
                // Can be set up to return 'null' if no account can be found
                account = _backup.GetAccount(request.DebtorAccountNumber);
            }

            if (account == null)
            {
                return new MakePaymentResult { Success = false, FailureMessage = "No matching account was found for the provided Debtor Account Number" };
            }

            var result = new MakePaymentResult();

            if(!_paymentRules.TryGetValue(request.PaymentScheme, out var paymentRule))
            {
                return new MakePaymentResult { Success = false, FailureMessage = $"Invalid Payment Scheme provided: {request.PaymentScheme}" };
            }

            result.Success = paymentRule.IsValid(account, request);

            if (result.Success)
            {
                account.Balance -= request.Amount;

                try
                {
                    _primary.UpdateAccount(account);
                }
                catch
                {
                    _backup.UpdateAccount(account);
                }
            }
            else
            {
                result.FailureMessage = $"Payment failed validation";
            }

            return result;
        }
    }
}
