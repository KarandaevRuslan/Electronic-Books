using ElectronicBooks.BookReference;
using ElectronicBooks.Heap.Models;
using ElectronicBooks.Heap.Other;
using ElectronicBooks.Heap.SQLiteEFClasses;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ElectronicBooks.Views.Windows
{
    public partial class Account : Window
    {
        private int id;
        public int currentMode;
        public int settingsState;

        public Account(int userId)
        {
            this.InitializeComponent();
            this.Closing += new CancelEventHandler(this.OnAccountWindowClosing);
            this.id = userId;
            this.UpdateNameAndDescription();
            this.UpdateBooksList();
        }

        private void AddListBoxItemInList(int bookId)
        {
            ListBoxItem newItem = new ListBoxItem();
            ServiceBookClient client1 = AppSettings.GetClient();
            client1.Open();
            string[] book = client1.GetBook(bookId);
            client1.Close();
            newItem.Content = (object)new TextBlock()
            {
                Text = book[0],
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new FontFamily("Times New Roman"),
                TextAlignment = TextAlignment.Center,
                FontSize = 15.0
            };
            newItem.BorderBrush = (Brush)null;
            newItem.MinWidth = 460.0;
            newItem.MaxWidth = 460.0;
            newItem.Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
            newItem.MouseLeftButtonUp += (MouseButtonEventHandler)((sender, e) =>
            {
                new BookDescription1(bookId).ShowDialog();
                this.list.SelectedIndex = -1;
            });
            newItem.MouseRightButtonUp += (MouseButtonEventHandler)((sender, e) =>
            {
                new SelectionInAccount() { Owner = ((Window)this) }.ShowDialog();
                switch (this.currentMode)
                {
                    case 1:
                        this.currentMode = 0;
                        new BookDescription1(bookId).ShowDialog();
                        break;
                    case 2:
                        this.currentMode = 0;
                        CommonFunctions.Get(bookId, this.id);
                        break;
                    case 3:
                        this.currentMode = 0;
                        ServiceBookClient client2 = AppSettings.GetClient();
                        client2.Open();
                        client2.DeleteBook(bookId);
                        client2.Close();
                        this.UpdateBooksList();
                        break;
                    case 4:
                        this.currentMode = 0;
                        new EditBookDescription1(bookId).ShowDialog();
                        this.UpdateBooksList();
                        break;
                }
                this.list.SelectedIndex = -1;
            });
            this.list.Items.Add((object)newItem);
        }

        private void UpdateNameAndDescription()
        {
            ServiceBookClient client = AppSettings.GetClient();
            client.Open();
            this.Login.Text = client.GetName(this.id);
            this.Description.Text = client.GetDescription(this.id);
            client.Close();
        }

        private void OnAccountWindowClosing(object sender, CancelEventArgs e)
        {
            new MainWindow().Show();
        }

        private void OnSettings(object sender, MouseButtonEventArgs e)
        {
            UserSettings userSettings = new UserSettings(this.id);
            userSettings.Owner = (Window)this;
            userSettings.ShowDialog();
            if (this.settingsState == 2)
            {
                AppSettings.UserId = 0;
                AppDBcontext appDbcontext = new AppDBcontext();
                appDbcontext.Settings.First<Settings>().UserId = 0;
                appDbcontext.SaveChanges();
                this.Close();
            }
            else
                this.UpdateNameAndDescription();
        }

        private void OnSearch(object sender, MouseButtonEventArgs e)
        {
            new AccountSearch(this.id).ShowDialog();
        }

        private void Back(object sender, MouseButtonEventArgs e) => this.Close();

        private void AddBook(object sender, MouseButtonEventArgs e)
        {
            new SelectBookToAdd(this.id).ShowDialog();
            this.UpdateBooksList();
        }

        private void UpdateBooksList()
        {
            this.list.Items.Clear();
            ServiceBookClient client = AppSettings.GetClient();
            client.Open();
            int[] booksId = client.GetBooksId(this.id);
            client.Close();
            foreach (int bookId in booksId)
                this.AddListBoxItemInList(bookId);
        }


    }
}
