using ElectronicBooks.BookReference;
using ElectronicBooks.Heap.Other;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ElectronicBooks.Views.Windows
{
    public partial class AccountSearch : Window
    {
        private int id;

        public AccountSearch(int userId)
        {
            this.InitializeComponent();
            this.id = userId;
            ServiceBookClient client = AppSettings.GetClient();
            client.Open();
            int[] numArray = client.SearchUsers(this.id, "");
            client.Close();
            foreach (int idSearchedUser in numArray)
                this.AddListBoxItemInList(idSearchedUser);
        }

        private void AddListBoxItemInList(int idSearchedUser)
        {
            ListBoxItem newItem = new ListBoxItem();
            ServiceBookClient client = AppSettings.GetClient();
            client.Open();
            string name = client.GetName(idSearchedUser);
            client.Close();
            newItem.Content = (object)new TextBlock()
            {
                Text = name,
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
                new Account1(idSearchedUser).ShowDialog();
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
            int[] numArray = client.SearchUsers(this.id, this.Name.Text);
            client.Close();
            foreach (int idSearchedUser in numArray)
                this.AddListBoxItemInList(idSearchedUser);
        }

        private void Search(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;
            this.Search();
        }

    }
}
