using ElectronicBooks.Heap.Models;
using ElectronicBooks.Heap.Other;
using ElectronicBooks.Heap.SQLiteEFClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ElectronicBooks.Views.Windows
{
    public partial class ClearUnusedDocuments : Window
    {
        private string bookPath;
        private DatabaseContext saved;

        public ClearUnusedDocuments(string bookPath)
        {
            this.InitializeComponent();
            this.bookPath = bookPath;
            this.saved = new DatabaseContext(bookPath + "\\database\\PagesDB.db");
            List<string> stringList = new List<string>();
            int num = this.saved.Pages.Count<Pages>();
            for (int i = 1; i <= num; i++)
            {
                string relativeLink = this.saved.Pages.Where<Pages>((Expression<Func<Pages, bool>>)(c => c.Id == i)).FirstOrDefault<Pages>().RelativeLink;
                if (relativeLink != null)
                {
                    string str = AnyConverter.EncodeAndDecodeUrl(relativeLink).Replace('/', '\\');
                    stringList.Add(bookPath + "\\data\\" + str);
                }
            }
            foreach (string pathRes in ((IEnumerable<string>)Directory.GetFiles(bookPath + "\\data\\", "*", SearchOption.AllDirectories)).ToList<string>())
            {
                if (!stringList.Contains(pathRes))
                    this.list.Items.Add((object)this.GetListBoxItemWithCommands(pathRes, 510));
            }
        }

        private void ClearAll(object sender, MouseButtonEventArgs e)
        {
            foreach (ListBoxItem listBoxItem in (IEnumerable)this.list.Items)
                this.Delete(listBoxItem, false);
            this.list.Items.Clear();
        }

        private void Delete(ListBoxItem listBoxItem, bool shouldListBoxItemBeRemoved)
        {
            File.Delete(((TextBlock)listBoxItem.Content).Text);
            if (!shouldListBoxItemBeRemoved)
                return;
            this.list.Items.Remove((object)listBoxItem);
            this.list.SelectedIndex = -1;
        }

        private ListBoxItem GetListBoxItemWithCommands(string pathRes, int buttonWidth)
        {
            ListBoxItem listBoxItem = this.GetListBoxItem(pathRes, buttonWidth);
            listBoxItem.MouseLeftButtonUp += (MouseButtonEventHandler)((sender, e) => this.Delete((ListBoxItem)sender, true));
            return listBoxItem;
        }

        private ListBoxItem GetListBoxItem(string pathRes, int buttonWidth)
        {
            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Content = (object)new TextBlock()
            {
                Text = pathRes,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new FontFamily("Times New Roman"),
                TextAlignment = TextAlignment.Center,
                FontSize = 15.0,
                Foreground = (Brush)AnyConverter.GetBrush("#000000")
            };
            listBoxItem.Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
            listBoxItem.BorderBrush = (Brush)null;
            listBoxItem.MinWidth = (double)buttonWidth;
            listBoxItem.MaxWidth = (double)buttonWidth;
            return listBoxItem;
        }
    }
}
