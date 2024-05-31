using ElectronicBooks.Heap.Other;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ElectronicBooks.Views.Windows
{
    public partial class BookNameEditor : Window
    {
        private string bookName = "";
        private string bookPath = "";


        public BookNameEditor(string bookPath)
        {
            this.InitializeComponent();
            this.bookPath = AnyConverter.GetShortDirectoryPath(bookPath);
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
            this.bookName = this.WhatIsBookName.Text;
            if (this.bookName.Equals("") || this.bookName == null)
            {
                int num = (int)MessageBox.Show("Ошибка ввода");
                this.bookName = "";
            }
            else if (this.bookName.Equals(AnyConverter.GetDirectoryName(this.bookPath)))
            {
                int num = (int)MessageBox.Show("Введите новое название");
                this.bookName = "";
            }
            else
            {
                string str = AppSettings.pathBookCatalog + "\\" + this.bookName;
                if (Directory.Exists(str))
                {
                    int num = (int)MessageBox.Show("Книга с данным названием уже существует");
                    this.bookName = "";
                }
                else
                {
                    try
                    {
                        Directory.Move(this.bookPath, str);
                        ((BookEditor)this.Owner).path = str;
                    }
                    catch (Exception ex)
                    {
                        int num = (int)MessageBox.Show("Произошла ошибка при попытке изменения названия книги. Решение: 1) попробуйте перезапустить книгу; 2) Измените название книги вручную и перезапустите книгу.", "Непредвиденная ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
                    }
                    this.Close();
                }
            }
        }
    }
}
