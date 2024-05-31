using ElectronicBooks.BookReference;
using ElectronicBooks.Heap.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ElectronicBooks.Views.Windows
{
    public partial class UserSettings : Window
    {
        private int id;

        public UserSettings(int userId)
        {
            this.InitializeComponent();
            this.id = userId;
            ServiceBookClient client = AppSettings.GetClient();
            client.Open();
            this.Login.Text = client.GetName(userId);
            this.Password.Text = client.GetPassword(userId);
            this.Description.Text = client.GetDescription(userId);
            client.Close();
        }

        private void Quit(object sender, RoutedEventArgs e)
        {
            ((Account)this.Owner).settingsState = 2;
            this.Close();
        }

        private void Remove(object sender, RoutedEventArgs e)
        {
            switch (MessageBox.Show("Вы действительно хотите безвозвратно удалить аккаунт и все связанные с ним данные?", "Удаление аккаунта", MessageBoxButton.YesNo, MessageBoxImage.Asterisk))
            {
                case MessageBoxResult.Yes:
                    ServiceBookClient client = AppSettings.GetClient();
                    client.Open();
                    client.DeleteAccount(this.id);
                    client.Close();
                    ((Account)this.Owner).settingsState = 2;
                    this.Close();
                    break;
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            ServiceBookClient client = AppSettings.GetClient();
            client.Open();
            int num1 = client.EditUserData(this.id, this.Login.Text, this.Password.Text, this.Description.Text);
            client.Close();
            if (num1 == 0)
            {
                int num2 = (int)MessageBox.Show("Пользователь с таким логином уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            else
            {
                if (num1 != -1)
                    return;
                int num3 = (int)MessageBox.Show("Некорректный логин.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }
    }
}
