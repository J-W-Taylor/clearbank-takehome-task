using System.Configuration;

namespace ClearBank.DeveloperTest.Data
{
    public class DataStoreProvider
    {
        public IAccountDataStore Primary { get; }
        public IAccountDataStore Backup { get; }

        public DataStoreProvider()
        {
            Primary = new AccountDataStore(
                ConfigurationManager.ConnectionStrings["MainDb"].ConnectionString);

            Backup = new AccountDataStore(
                ConfigurationManager.ConnectionStrings["BackupDb"].ConnectionString);
        }
    }
}
