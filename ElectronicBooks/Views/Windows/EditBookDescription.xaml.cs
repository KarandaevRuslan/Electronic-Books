using ElectronicBooks.Heap.Models;
using ElectronicBooks.Heap.SQLiteEFClasses;
using System.Linq;
using System.Windows;

namespace ElectronicBooks.Views.Windows
{
    public partial class EditBookDescription : Window
    {
        private DatabaseContext context;
        private string bookName;

        public EditBookDescription(DatabaseContext context, string bookName)
        {
            this.InitializeComponent();
            this.context = context;
            this.Description.Text = context.BookData.First<BookData>().Description;
            this.UserName.Text = context.BookData.First<BookData>().UserName;
            this.Name.Text = bookName;
            this.bookName = bookName;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            this.context.BookData.First<BookData>().UserName = this.UserName.Text;
            this.context.BookData.First<BookData>().Description = this.Description.Text;
            this.context.SaveChanges();
        }
    }
}
