using ElectronicBooks.BookReference;
using ElectronicBooks.Heap.Other;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ElectronicBooks.Views.Windows
{
    public partial class BookSearch : Window
    {
        private int id;
        public int currentMode;

        public BookSearch(int userId)
        {
            this.InitializeComponent();
            this.Closing += new CancelEventHandler(this.OnWindowClosing);
            this.id = userId;
            ServiceBookClient client = AppSettings.GetClient();
            client.Open();
            int[] numArray = client.SearchBooks(this.id, "");
            client.Close();
            foreach (int idSearchedBook in numArray)
                this.AddListBoxItemInList(idSearchedBook);
        }

        private void OnWindowClosing(object sender, CancelEventArgs e) => new MainWindow().Show();

        private void AddListBoxItemInList(int idSearchedBook)
        {
            ListBoxItem newItem = new ListBoxItem();
            ServiceBookClient client1 = AppSettings.GetClient();
            client1.Open();
            string[] book = client1.GetBook(idSearchedBook);
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
            newItem.MinWidth = 390.0;
            newItem.MaxWidth = 390.0;
            newItem.Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
            newItem.MouseLeftButtonUp += (MouseButtonEventHandler)((sender, e) =>
            {
                new BookDescription1(idSearchedBook).ShowDialog();
                this.list.SelectedIndex = -1;
            });
            newItem.MouseRightButtonUp += (MouseButtonEventHandler)((sender, e) =>
            {
                new SelectionInAccount2()
                {
                    Owner = ((Window)this)
                }.ShowDialog();
                switch (this.currentMode)
                {
                    case 1:
                        this.currentMode = 0;
                        new BookDescription1(idSearchedBook).ShowDialog();
                        break;
                    case 2:
                        this.currentMode = 0;
                        ServiceBookClient client2 = AppSettings.GetClient();
                        client2.Open();
                        int userId1 = client2.GetUserId(idSearchedBook);
                        client2.Close();
                        CommonFunctions.Get(idSearchedBook, userId1);
                        break;
                    case 3:
                        this.currentMode = 0;
                        ServiceBookClient client3 = AppSettings.GetClient();
                        client3.Open();
                        int userId2 = client3.GetUserId(idSearchedBook);
                        client3.Close();
                        new Account1(userId2).ShowDialog();
                        break;
                }
                this.list.SelectedIndex = -1;
            });
            this.list.Items.Add((object)newItem);
        }

        private void Search(object sender, RoutedEventArgs e) => this.Search();

        private void Search()
        {
            this.list.Items.Clear();
            ServiceBookClient client = AppSettings.GetClient();
            client.Open();
            int[] numArray = client.SearchBooks(this.id, this.Name.Text);
            client.Close();
            foreach (int idSearchedBook in numArray)
                this.AddListBoxItemInList(idSearchedBook);
        }

        private void Search(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;
            this.Search();
        }
    }
}
