
using System.ComponentModel.DataAnnotations;
using System.Data.Linq.Mapping;

namespace ElectronicBooks.Heap.Models
{
  [Table(Name = "BookData")]
  public class BookData
  {
    [Column(Name = "Id", IsPrimaryKey = true, DbType = "INTEGER", CanBeNull = false)]
    [Key]
    public int Id { get; set; }

    [Column(Name = "UserName", DbType = "TEXT", CanBeNull = false)]
    public string UserName { get; set; }

    [Column(Name = "Description", DbType = "TEXT", CanBeNull = false)]
    public string Description { get; set; }

    [Column(Name = "CanEdit", DbType = "INTEGER", CanBeNull = false)]
    public int CanEdit { get; set; }

    [Column(Name = "PasswordText", DbType = "TEXT", CanBeNull = false)]
    public string PasswordText { get; set; }

    [Column(Name = "WasActivated", DbType = "INTEGER", CanBeNull = false)]
    public int WasActivated { get; set; }
  }
}
