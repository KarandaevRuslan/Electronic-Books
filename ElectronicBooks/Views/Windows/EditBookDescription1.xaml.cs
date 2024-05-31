using ElectronicBooks.BookReference;
using ElectronicBooks.Heap.Other;
using System;
using System.Windows;

namespace ElectronicBooks.Views.Windows
{
    public partial class EditBookDescription1 : Window
    {
        private int id;

        public EditBookDescription1(int bookId)
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
            this.id = bookId;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            ServiceBookClient client = AppSettings.GetClient();
            client.Open();
            string[] book = client.GetBook(this.id);
            if (!book[0].Equals(this.Name.Text))
            {
                switch (client.EditBookName(this.id, this.Name.Text))
                {
                    case -1:
                        int num1 = (int)MessageBox.Show("Книга с таким названием уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
                        break;
                    case 0:
                        int num2 = (int)MessageBox.Show("Некорректное название книги.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
                        break;
                }
            }
            if (!book[1].Equals(this.Description.Text))
                client.EditBookDescription(this.id, this.Description.Text);
            int canEdit = -1;
            if ("Можно".Equals(this.CanEdit.Text))
                canEdit = 1;
            else if ("Нельзя".Equals(this.CanEdit.Text))
                canEdit = 0;
            if (canEdit == -1)
            {
                int num3 = (int)MessageBox.Show("Параметр \"Можно ли редактировать\" принимает лишь 2 значения: \"Можно\" и \"Нельзя\".", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            else if (Convert.ToInt32(book[3]) != canEdit)
                client.EditBookCanEdit(this.id, canEdit);
            int access = -1;
            if ("Публичный".Equals(this.Access.Text))
                access = 1;
            else if ("Частный".Equals(this.Access.Text))
                access = 0;
            if (access == -1)
            {
                int num4 = (int)MessageBox.Show("Параметр \"Доступ\" принимает лишь 2 значения: \"Публичный\" и \"Частный\".", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            else if (Convert.ToInt32(book[4]) != access)
                client.EditBookAccess(this.id, access);
            client.Close();
        }
    }
}
