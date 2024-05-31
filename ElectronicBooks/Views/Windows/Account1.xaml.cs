using ElectronicBooks.BookReference;
using ElectronicBooks.Heap.Other;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ElectronicBooks.Views.Windows
{
    public partial class Account1 : Window
    {
        private int id;
        public int currentMode;
        public int settingsState;

        public Account1(int userId)
        {
            this.InitializeComponent();
            this.id = userId;
            this.UpdateNameAndDescription();
            this.UpdateBooksList();
        }

        private void AddListBoxItemInList(int bookId)
        {
            ListBoxItem newItem = new ListBoxItem();
            ServiceBookClient client = AppSettings.GetClient();
            client.Open();
            string[] book = client.GetBook(bookId);
            if (Convert.ToInt32(book[4]) == 0)
            {
                client.Close();
            }
            else
            {
                client.Close();
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
                    new SelectionInAccount1()
                    {
                        Owner = ((Window)this)
                    }.ShowDialog();
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
                    }
                    this.list.SelectedIndex = -1;
                });
                this.list.Items.Add((object)newItem);
            }
        }

        private void UpdateNameAndDescription()
        {
            ServiceBookClient client = AppSettings.GetClient();
            client.Open();
            this.Login.Text = client.GetName(this.id);
            this.Description.Text = client.GetDescription(this.id);
            client.Close();
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
