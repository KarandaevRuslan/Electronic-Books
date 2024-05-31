using System.ComponentModel.DataAnnotations;
using System.Data.Linq.Mapping;

namespace ElectronicBooks.Heap.Models
{
  [Table(Name = "Settings")]
  public class Settings
  {
    [Column(Name = "Id", IsPrimaryKey = true, DbType = "INTEGER", CanBeNull = false)]
    [Key]
    public int Id { get; set; }

    [Column(Name = "BookCatalogPath", DbType = "TEXT", CanBeNull = false)]
    public string BookCatalogPath { get; set; }

    [Column(Name = "HostIP", DbType = "TEXT", CanBeNull = false)]
    public string HostIP { get; set; }

    [Column(Name = "UserId", DbType = "INTEGER", CanBeNull = false)]
    public int UserId { get; set; }
  }
}
