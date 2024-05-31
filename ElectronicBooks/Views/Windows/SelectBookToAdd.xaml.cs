using ElectronicBooks.BookReference;
using ElectronicBooks.Heap.Models;
using ElectronicBooks.Heap.Other;
using ElectronicBooks.Heap.SQLiteEFClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ElectronicBooks.Views.Windows
{
    public partial class SelectBookToAdd : Window
    {
        private int id;

        public SelectBookToAdd(int userId)
        {
            this.InitializeComponent();
            this.id = userId;
            this.FillListBox();
        }

        private void AddListBoxItemInList(string str)
        {
            ListBoxItem newItem = new ListBoxItem();
            newItem.Content = (object)new TextBlock()
            {
                Text = ((IEnumerable<string>)str.Split('\\')).Last<string>(),
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
                string str1 = AppSettings.pathBookCatalog + "\\" + ((TextBlock)((ContentControl)sender).Content).Text;
                DatabaseContext databaseContext = new DatabaseContext(str1 + "\\database\\PagesDB.db");
                ServiceBookClient client = AppSettings.GetClient();
                client.Open();
                File.Copy(str1 + "\\database\\PagesDB.db", str1 + "\\database\\copy.db");
                File.Copy(str1 + "\\database\\UnsavedPagesDB.db", str1 + "\\database\\copy1.db");
                List<FileTransferRequest> fileTransferRequestList = new List<FileTransferRequest>();
                foreach (string file in Directory.GetFiles(str1 + "\\database", "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        FileTransferRequest fileTransferRequest = new FileTransferRequest()
                        {
                            FileName = AnyConverter.Get_Directory_Name_Extension_FromFilePath(file)[1] + "." + AnyConverter.Get_Directory_Name_Extension_FromFilePath(file)[2],
                            Content = File.ReadAllBytes(file)
                        };
                        if (AnyConverter.Get_Directory_Name_Extension_FromFilePath(file)[1].Equals("copy"))
                        {
                            fileTransferRequest.FileName = "PagesDB.db";
                            fileTransferRequestList.Add(fileTransferRequest);
                        }
                        else if (AnyConverter.Get_Directory_Name_Extension_FromFilePath(file)[1].Equals("copy1"))
                        {
                            fileTransferRequest.FileName = "UnsavedPagesDB.db";
                            fileTransferRequestList.Add(fileTransferRequest);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                File.Delete(str1 + "\\database\\copy.db");
                File.Delete(str1 + "\\database\\copy1.db");
                Task<int> task = client.PutAsync(fileTransferRequestList.ToArray(), ((TextBlock)((ContentControl)sender).Content).Text, databaseContext.BookData.First<BookData>().Description, this.id);
                try
                {
                    task.Wait();
                }
                catch (Exception ex){
                    MessageBox.Show("Книга слишком большая");
                    return;
                }
                int result = task.Result;
                foreach (string file in Directory.GetFiles(str1 + "\\data", "*", SearchOption.AllDirectories))
                {
                    FileTransferRequest fileToPush = new FileTransferRequest()
                    {
                        FileName = AnyConverter.Get_Directory_Name_Extension_FromFilePath(file)[1] + "." + AnyConverter.Get_Directory_Name_Extension_FromFilePath(file)[2],
                        Content = File.ReadAllBytes(file)
                    };
                    string path = AnyConverter.Get_Directory_Name_Extension_FromFilePath(file.Substring(str1.Length + 1))[0];
                    client.Put1Async(fileToPush, result, this.id, path).Wait();
                }
                client.Close();
                this.Close();
            });
            this.list.Items.Add((object)newItem);
        }

        private void FillListBox()
        {
            try
            {
                string pathBookCatalog = AppSettings.pathBookCatalog;
                if (!Directory.Exists(pathBookCatalog))
                    Directory.CreateDirectory(pathBookCatalog);
                foreach (string directory in Directory.GetDirectories(pathBookCatalog))
                {
                    if (new DatabaseContext(directory + "\\database\\PagesDB.db").BookData.First<BookData>().CanEdit == 1)
                        this.AddListBoxItemInList(directory);
                }
            }
            catch (Exception ex)
            {
                this.Close();
            }
        }
    }
}
