using ElectronicBooks.BookReference;
using ElectronicBooks.Heap.Models;
using ElectronicBooks.Heap.Other;
using ElectronicBooks.Heap.SQLiteEFClasses;
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
    public partial class Registration : Window
    {
        private AppDBcontext appDBcontext;

        public Registration()
        {
            this.InitializeComponent();
            this.appDBcontext = new AppDBcontext();
        }

        private void Register(object sender, RoutedEventArgs e)
        {
            ServiceBookClient client = AppSettings.GetClient();
            try
            {
                client.Open();
                int num1 = client.RegisterUser(this.Login.Text, this.Password.Text);
                client.Close();
                if (num1 == 0)
                {
                    int num2 = (int)MessageBox.Show("Пользователь с таким логином уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
                }
                else if (num1 == -1)
                {
                    int num3 = (int)MessageBox.Show("Некорректный логин.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
                }
                else
                {
                    AppSettings.UserId = num1;
                    this.appDBcontext.Settings.First<Settings>().UserId = num1;
                    this.appDBcontext.SaveChanges();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show("Нет подключения к серверу. Попробуйте подключиться позже.", "Ошибка соединения", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }
    }
}
