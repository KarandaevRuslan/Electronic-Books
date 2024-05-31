using ElectronicBooks.Heap.Models;
using ElectronicBooks.Heap.SQLiteEFClasses;
using System.Linq;
using System.Windows;

namespace ElectronicBooks.Views.Windows
{
    public partial class BookDescription : Window
    {

        public BookDescription(DatabaseContext context, string bookName)
        {
            this.InitializeComponent();
            this.Description.Text = context.BookData.First<BookData>().Description;
            this.UserName.Text = context.BookData.First<BookData>().UserName;
            this.Name.Text = bookName;
        }
    }
}
