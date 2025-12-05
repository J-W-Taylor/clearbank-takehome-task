using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services.PaymentRules;
using ClearBank.DeveloperTest.Types;
using System;
using System.Collections.Generic;

namespace ClearBank.DeveloperTest.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAccountDataStore _primary;
        private readonly IAccountDataStore _backup;

        private readonly Dictionary<PaymentScheme, IPaymentRule> _paymentRules = new();

        public PaymentService(DataStoreFactory provider, IEnumerable<IPaymentRule> paymentRules)
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
                account = _primary.GetAccount(request.DebtorAccountNumber);
            }
            catch
            {
                account = _backup.GetAccount(request.DebtorAccountNumber);
            }

            var result = new MakePaymentResult();

            if(!_paymentRules.TryGetValue(request.PaymentScheme, out var paymentRule))
            {
                throw new ArgumentException($"Invalid Payment Scheme provided: {request.PaymentScheme}");
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

            return result;
        }
    }
}
