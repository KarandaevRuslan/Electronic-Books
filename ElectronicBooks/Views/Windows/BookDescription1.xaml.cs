using ElectronicBooks.BookReference;
using ElectronicBooks.Heap.Other;
using System.Windows;

namespace ElectronicBooks.Views.Windows
{
    public partial class BookDescription1 : Window
    {

        public BookDescription1(int bookId)
        {
            this.InitializeComponent();
            ServiceBookClient client = AppSettings.GetClient();
            client.Open();
            string[] book = client.GetBook(bookId);
            this.Description.Text = book[1];
            this.UserName.Text = book[2];
            this.Name.Text = book[0];
            this.CanEdit.Text = !book[3].Equals("0") ? "Можно" : "Нельзя";
            this.Access.Text = !book[4].Equals("0") ? "Публичный" : "Частный";
            client.Close();
        }
    }
}
