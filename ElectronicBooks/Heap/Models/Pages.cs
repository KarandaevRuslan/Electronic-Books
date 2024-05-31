using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Linq.Mapping;

namespace ElectronicBooks.Heap.Models
{
  [Table(Name = "Pages")]
  public class Pages : ICloneable
  {
    [Column(Name = "Id", IsPrimaryKey = true, DbType = "INTEGER", CanBeNull = false)]
    [Key]
    public int Id { get; set; }

    [Column(Name = "PageOrder", DbType = "INTEGER", CanBeNull = false)]
    public int PageOrder { get; set; }

    [Column(Name = "Title", DbType = "TEXT", CanBeNull = false)]
    public string Title { get; set; }

    [Column(Name = "AbsoluteLink", DbType = "TEXT", CanBeNull = true)]
    public string AbsoluteLink { get; set; }

    [Column(Name = "RelativeLink", DbType = "TEXT", CanBeNull = true)]
    public string RelativeLink { get; set; }

    [Column(Name = "Color", DbType = "TEXT", CanBeNull = false)]
    public string Color { get; set; }

    [Column(Name = "IsTitle", DbType = "INTEGER", CanBeNull = false)]
    public int IsTitle { get; set; }

    public object Clone()
    {
      return (object) new Pages()
      {
        PageOrder = this.PageOrder,
        Title = this.Title,
        AbsoluteLink = this.AbsoluteLink,
        RelativeLink = this.RelativeLink,
        Color = this.Color,
        IsTitle = this.IsTitle
      };
    }
  }
}
