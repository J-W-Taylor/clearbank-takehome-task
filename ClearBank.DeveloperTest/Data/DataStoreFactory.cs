using System.Configuration;

namespace ClearBank.DeveloperTest.Data
{
    public class DataStoreFactory
    {
        public IAccountDataStore Primary { get; }
        public IAccountDataStore Backup { get; }

        public DataStoreFactory()
        {
            Primary = new AccountDataStore(
                ConfigurationManager.ConnectionStrings["MainDb"].ConnectionString);

            Backup = new AccountDataStore(
                ConfigurationManager.ConnectionStrings["BackupDb"].ConnectionString);
        }
    }
}
