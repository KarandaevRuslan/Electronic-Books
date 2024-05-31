
using System.ComponentModel.DataAnnotations;
using System.Data.Linq.Mapping;

namespace Databases.Models
{
    [Table(Name = "Users")]
    public class User
    {
        [Column(Name = "Id", IsPrimaryKey = true, DbType = "INTEGER", CanBeNull = false)]
        [Key]
        public int Id { get; set; }

        [Column(Name = "Login", DbType = "TEXT", CanBeNull = false)]
        public string Login { get; set; }

        [Column(Name = "Password", DbType = "TEXT", CanBeNull = false)]
        public string Password { get; set; }

        [Column(Name = "UserInformation", DbType = "TEXT", CanBeNull = false)]
        public string UserInformation { get; set; }
    }
}
