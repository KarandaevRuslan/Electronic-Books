using ElectronicBooks.Heap.Models;
using ElectronicBooks.Heap.Other;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;

namespace ElectronicBooks.Heap.SQLiteEFClasses
{
    internal class AppDBcontext : DbContext
    {

        public AppDBcontext() : base(new SQLiteConnection(new SQLiteConnectionStringBuilder() { DataSource = AppSettings.AppDBPath(), ForeignKeys = true }.ConnectionString), true)
        {
            this.Database.Connection.Open();
            if (this.IsEmptyTable(nameof(Settings)))
            {
                DbCommand command = this.Database.Connection.CreateCommand();
                command.CommandText = "CREATE TABLE \"Settings\" (\"Id\"\tINTEGER NOT NULL,\"BookCatalogPath\"\tTEXT NOT NULL,\"HostIP\"\tTEXT NOT NULL,\"UserId\" INTEGER NOT NULL,PRIMARY KEY(\"Id\"))";
                command.ExecuteNonQuery();
            }
            this.Database.Connection.Close();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Settings> Settings { get; set; }

        private bool IsEmptyTable(string tableName)
        {
            DbCommand command = this.Database.Connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = \"table\" AND name = \"" + tableName + "\"";
            DbDataReader dbDataReader = command.ExecuteReader();
            int num = 0;
            if (dbDataReader.Read())
                num = dbDataReader.GetInt32(0);
            dbDataReader.Close();
            return num == 0;
        }
    }
}
