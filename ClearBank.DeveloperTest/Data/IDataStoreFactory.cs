using System.Configuration;

namespace ClearBank.DeveloperTest.Data
{
    public interface IDataStoreFactory
    {
        public IAccountDataStore Primary { get; }
        public IAccountDataStore Backup { get; }
    }
}
