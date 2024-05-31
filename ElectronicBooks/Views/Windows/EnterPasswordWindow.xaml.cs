using ElectronicBooks.Heap.Models;
using ElectronicBooks.Heap.SQLiteEFClasses;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ElectronicBooks.Views.Windows
{
    public partial class EnterPasswordWindow : Window
    {
        private DatabaseContext contextSaved;

        public EnterPasswordWindow(string path)
        {
            this.InitializeComponent();
            this.contextSaved = new DatabaseContext(path + "\\database\\PagesDB.db");
        }

        private void Enter(object sender, RoutedEventArgs e) => this.SubEnter();

        private void Enter(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;
            this.SubEnter();
        }

        private void SubEnter()
        {
            if (!this.contextSaved.BookData.FirstOrDefault<BookData>().PasswordText.Equals(this.WhatIsPassword.Text))
            {
                int num = (int)MessageBox.Show("Вы ввели неверный пароль!", "Неверный пароль", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            else
            {
                if (this.Owner is MainWindow)
                    ((MainWindow)this.Owner).canGoToEditor = true;
                else
                    ((BookViewer)this.Owner).canGoToEditor = true;
                this.Close();
            }
        }
    }
}
