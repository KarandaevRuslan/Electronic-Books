using ElectronicBooks.BookReference;
using ElectronicBooks.Heap.Models;
using ElectronicBooks.Heap.Other;
using ElectronicBooks.Heap.SQLiteEFClasses;
using System;
using System.Linq;
using System.Windows;

namespace ElectronicBooks.Views.Windows
{
    public partial class Authentication : Window
    {
        private AppDBcontext appDBcontext;

        public Authentication()
        {
            this.InitializeComponent();
            this.appDBcontext = new AppDBcontext();
        }

        private void Sign_in(object sender, RoutedEventArgs e)
        {
            ServiceBookClient client = AppSettings.GetClient();
            try
            {
                client.Open();
                int num1 = client.SignIn(this.Login.Text, this.Password.Text);
                client.Close();
                if (num1 == 0)
                {
                    int num2 = (int)MessageBox.Show("Вы ввели неверный логин или пароль!", "Неверный логин или пароль", MessageBoxButton.OK, MessageBoxImage.Hand);
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

        private void OpenRegistrationDialog(object sender, RoutedEventArgs e)
        {
            ((MainWindow)this.Owner).canGoToRegistration = true;
            this.Close();
        }
    }
}
