using ElectronicBooks.Heap.Other;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ElectronicBooks.Views.Windows
{
    public partial class BookName : Window
    {
        private string bookName = "";

        public BookName() => this.InitializeComponent();

        private void Enter(object sender, RoutedEventArgs e) => this.SubEnter();

        private void Enter(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;
            this.SubEnter();
        }

        private void SubEnter()
        {
            this.bookName = this.WhatIsBookName.Text;
            if (this.bookName.Equals("") || this.bookName == null)
            {
                int num = (int)MessageBox.Show("Ошибка ввода");
                this.bookName = "";
            }
            else
            {
                string path = AppSettings.pathBookCatalog + "\\" + this.bookName;
                if (Directory.Exists(path))
                {
                    int num = (int)MessageBox.Show("Книга с данным названием уже существует");
                    this.bookName = "";
                }
                else
                {
                    Directory.CreateDirectory(path);
                    Directory.CreateDirectory(path + "\\data");
                    Directory.CreateDirectory(path + "\\database");
                    try
                    {
                        ((MainWindow)this.Owner).currentName = this.bookName;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        int num = (int)MessageBox.Show(ex.Message);
                    }
                }
            }
        }
    }
}
