using ElectronicBooks.Heap.Models;
using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;

namespace ElectronicBooks.Heap.SQLiteEFClasses
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(string connectionSource) : base(new SQLiteConnection(new SQLiteConnectionStringBuilder()
        {
            DataSource = connectionSource,
            ForeignKeys = true
        }.ConnectionString), true)
        {
            this.Database.Connection.Open();
            if (this.IsEmptyTable(nameof(Pages)))
            {
                DbCommand command = this.Database.Connection.CreateCommand();
                command.CommandText = "CREATE TABLE \"Pages\" (\"Id\" INTEGER NOT NULL,\"PageOrder\" INTEGER NOT NULL,\"Title\" TEXT NOT NULL,\"AbsoluteLink\"  TEXT,\"RelativeLink\"  TEXT,\"Color\" TEXT NOT NULL,\"IsTitle\"   INTEGER NOT NULL,PRIMARY KEY(\"Id\"))";
                command.ExecuteNonQuery();
            }
            if (this.IsEmptyTable(nameof(BookData)))
            {
                DbCommand command = this.Database.Connection.CreateCommand();
                command.CommandText = "CREATE TABLE \"BookData\"(\"Id\"INTEGER NOT NULL,\"UserName\"  TEXT NOT NULL,\"Description\"  TEXT NOT NULL,\"CanEdit\"  INTEGER NOT NULL,\"PasswordText\"  TEXT NOT NULL,\"WasActivated\"  INTEGER NOT NULL,PRIMARY KEY(\"Id\"));";
                command.ExecuteNonQuery();
            }
            this.Database.Connection.Close();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Pages> Pages { get; set; }

        public DbSet<BookData> BookData { get; set; }

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

        public List<Pages> GetPages()
        {
            try
            {
                return this.Pages.OrderBy<Pages, int>((Expression<Func<Pages, int>>)(x => x.PageOrder)).ToList<Pages>();
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message);
                return (List<Pages>)null;
            }
        }

        public int GetNextPagesTitleOrder(int startOrder)
        {
            try
            {
                return this.Pages.OrderBy<Pages, int>((x => x.PageOrder)).Where<Pages>((x => x.IsTitle == 1 && x.PageOrder > startOrder)).FirstOrDefault<Pages>().PageOrder;
            }
            catch (Exception ex)
            {
                return this.GetPagesCount() + 1;
            }
        }

        public int GetPagesCount() => this.Pages.Count<Pages>();

        public Pages GetPage(int pOrder)
        {
            try
            {
                return (Pages)this.Pages.FirstOrDefault<Pages>((Expression<Func<Pages, bool>>)(x => x.PageOrder == pOrder)).Clone();
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message);
                return (Pages)null;
            }
        }
    }
}
