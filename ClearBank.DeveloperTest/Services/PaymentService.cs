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
            var debtorAccount = GetAccount(request.DebtorAccountNumber);
            var creditorAccount = GetAccount(request.CreditorAccountNumber);

            if (debtorAccount == null)
            {
                return new MakePaymentResult { Success = false, FailureMessage = "No matching account was found for the provided Debtor Account Number" };
            }

            if (creditorAccount == null)
            {
                return new MakePaymentResult { Success = false, FailureMessage = "No matching account was found for the provided Creditor Account Number" };
            }

            var result = new MakePaymentResult();

            if(!_paymentRules.TryGetValue(request.PaymentScheme, out var paymentRule))
            {
                return new MakePaymentResult { Success = false, FailureMessage = $"Invalid Payment Scheme provided: {request.PaymentScheme}" };
            }

            result.Success = paymentRule.IsValid(debtorAccount, request);

            if (result.Success)
            {
                debtorAccount.Balance -= request.Amount;
                creditorAccount.Balance += request.Amount;

                UpdateAccount(debtorAccount);
                UpdateAccount(creditorAccount);
            }
            else
            {
                result.FailureMessage = $"Payment failed validation";
            }

            return result;
        }

        private Account GetAccount(string accountNumber)
        {
            try
            {
                // Can be set up to return 'null' if no account can be found
                return _primary.GetAccount(accountNumber);
            }
            catch
            {
                // Can be set up to return 'null' if no account can be found
                return _backup.GetAccount(accountNumber);
            }
        }

        private void UpdateAccount(Account account)
        {
            try
            {
                _primary.UpdateAccount(account);
            }
            catch
            {
                _backup.UpdateAccount(account);
            }
        }
    }
}
