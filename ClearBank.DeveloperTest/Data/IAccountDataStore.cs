using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Data
{
    public interface IAccountDataStore
    {
        public Account GetAccount(string accountNumber);

        public void UpdateAccount(Account account);
    }
}
