using ElectronicBooks.Heap.Models;
using ElectronicBooks.Heap.SQLiteEFClasses;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ElectronicBooks.Views.Windows
{
    public partial class PasswordWindow : Window
    {
        private DatabaseContext contextSaved;

        public PasswordWindow(string path)
        {
            this.InitializeComponent();
            this.contextSaved = new DatabaseContext(path + "\\database\\PagesDB.db");
            if (this.contextSaved.BookData.First<BookData>().PasswordText.Equals(""))
            {
                this.SetAllInactive();
            }
            else
            {
                this.delete.Source = (ImageSource)new BitmapImage(new Uri("/ElectronicBooks;component/Resources/Images/trashcan.png", UriKind.Relative));
                BookData bookData = this.contextSaved.BookData.FirstOrDefault<BookData>();
                this.passwordText.Text = bookData.PasswordText;
                this.SetApplyOrCancel(bookData.WasActivated);
            }
        }

        private void Back(object sender, MouseButtonEventArgs e) => this.Close();

        private void ApplyOrCancel(object sender, MouseButtonEventArgs e)
        {
            if (this.contextSaved.BookData.First<BookData>().PasswordText.Equals(""))
                return;
            switch (this.contextSaved.BookData.First<BookData>().WasActivated)
            {
                case 0:
                    this.contextSaved.BookData.First<BookData>().WasActivated = 1;
                    break;
                case 1:
                    this.contextSaved.BookData.First<BookData>().WasActivated = 0;
                    break;
            }
            this.contextSaved.SaveChanges();
            this.SetApplyOrCancel(this.contextSaved.BookData.First<BookData>().WasActivated);
        }

        private void Remove(object sender, MouseButtonEventArgs e)
        {
            if (this.contextSaved.BookData.First<BookData>().PasswordText.Equals(""))
                return;
            this.passwordText.Text = "";
            this.SetAllInactive();
            this.contextSaved.BookData.First<BookData>().WasActivated = 0;
            this.contextSaved.BookData.First<BookData>().PasswordText = "";
            this.contextSaved.SaveChanges();
        }

        private void Save(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;
            this.Save();
        }

        private void Save(object sender, RoutedEventArgs e) => this.Save();

        private void Save()
        {
            if (this.passwordText.Text.Equals(""))
                return;
            if (this.contextSaved.BookData.First<BookData>().PasswordText == "")
            {
                this.contextSaved.BookData.First<BookData>().WasActivated = 0;
                this.delete.Source = (ImageSource)new BitmapImage(new Uri("/ElectronicBooks;component/Resources/Images/trashcan.png", UriKind.Relative));
                this.SetApplyOrCancel(0);
            }
            this.contextSaved.BookData.First<BookData>().PasswordText = this.passwordText.Text;
            this.contextSaved.SaveChanges();
        }

        private void SetAllInactive()
        {
            this.applyOrCancel.Source = (ImageSource)new BitmapImage(new Uri("/ElectronicBooks;component/Resources/Images/inactiveapplyicon.png", UriKind.Relative));
            this.delete.Source = (ImageSource)new BitmapImage(new Uri("/ElectronicBooks;component/Resources/Images/inactivetrashcan.png", UriKind.Relative));
            this.applyOrCancel.ToolTip = (object)"Чтобы активировать пароль, сначала создайте его";
        }

        private void SetApplyOrCancel(int wasActivated)
        {
            if (wasActivated != 0)
            {
                if (wasActivated != 1)
                    return;
                this.applyOrCancel.Source = (ImageSource)new BitmapImage(new Uri("/ElectronicBooks;component/Resources/Images/cancel.png", UriKind.Relative));
                this.applyOrCancel.ToolTip = (object)"Деактивировать пароль";
            }
            else
            {
                this.applyOrCancel.Source = (ImageSource)new BitmapImage(new Uri("/ElectronicBooks;component/Resources/Images/applyicon.png", UriKind.Relative));
                this.applyOrCancel.ToolTip = (object)"Активировать пароль";
            }
        }
    }
}
