using System.Data.SQLite;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.SQLite.EF6;

namespace ElectronicBooks.Heap.SQLiteEFClasses
{
    public class SQLiteConfiguration : DbConfiguration
    {
        public SQLiteConfiguration()
        {
            this.SetProviderFactory("System.Data.SQLite", (DbProviderFactory)SQLiteFactory.Instance);
            this.SetProviderFactory("System.Data.SQLite.EF6", (DbProviderFactory)SQLiteProviderFactory.Instance);
            this.SetProviderServices("System.Data.SQLite", (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices)));
        }
    }
}
