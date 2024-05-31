
using System.ComponentModel.DataAnnotations;
using System.Data.Linq.Mapping;

namespace Databases.Models
{
  [Table(Name = "Books")]
  public class Book
  {
    [Column(Name = "Id", IsPrimaryKey = true, DbType = "INTEGER", CanBeNull = false)]
    [Key]
    public int Id { get; set; }

    [Column(Name = "Name", DbType = "TEXT", CanBeNull = false)]
    public string Name { get; set; }

    [Column(Name = "Description", DbType = "TEXT", CanBeNull = false)]
    public string Description { get; set; }

    [Column(Name = "UserId", DbType = "INTEGER", CanBeNull = false)]
    public int UserId { get; set; }

    [Column(Name = "CanEdit", DbType = "INTEGER", CanBeNull = false)]
    public int CanEdit { get; set; }

    [Column(Name = "Access", DbType = "INTEGER", CanBeNull = false)]
    public int Access { get; set; }
  }
}
