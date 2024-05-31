using Databases.Models;
using System.Data.SQLite;
using System;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;

namespace Databases.SQLiteClasses
{
    public class StorageDBcontext : DbContext
    {
        public StorageDBcontext() : base(new SQLiteConnection(new SQLiteConnectionStringBuilder()
        {
            DataSource = AppSettings.StorageDBPath(),
            ForeignKeys = true
        }.ConnectionString), true)
        {
            Database.Connection.Open();
            if (this.IsEmptyTable(nameof(Users)))
            {
                DbCommand command = this.Database.Connection.CreateCommand();
                command.CommandText = "CREATE TABLE \"Users\" (\"Id\" INTEGER NOT NULL,\"Login\"TEXT NOT NULL,\"Password\"TEXT NOT NULL,\"UserInformation\"TEXT NOT NULL,PRIMARY KEY(\"Id\"));";
                command.ExecuteNonQuery();
            }
            if (this.IsEmptyTable(nameof(Books)))
            {
                DbCommand command = this.Database.Connection.CreateCommand();
                command.CommandText = "CREATE TABLE \"Books\" (\"Id\" INTEGER NOT NULL,\"Name\"TEXT NOT NULL,\"Description\"TEXT NOT NULL,\"UserId\"INTEGER NOT NULL,\"CanEdit\"INTEGER NOT NULL,\"Access\"INTEGER NOT NULL,PRIMARY KEY(\"Id\"));";
                command.ExecuteNonQuery();
            }
            this.Database.Connection.Close();
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Book> Books { get; set; }

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

        public int GetUsersCount() => this.Users.Count<User>();

        public int NewUserId()
        {
            int num = 1;
            try
            {
                num = this.Users.ToList<User>().Last<User>().Id + 1;
            }
            catch (Exception ex)
            {
            }
            return num;
        }

        public int NewBookId()
        {
            int num = 1;
            try
            {
                num = this.Books.ToList<Book>().Last<Book>().Id + 1;
            }
            catch (Exception ex)
            {
            }
            return num;
        }
    }
}
