using ElectronicBooks.BookReference;
using ElectronicBooks.Heap.Models;
using ElectronicBooks.Heap.SQLiteEFClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ElectronicBooks.Heap.Other
{
    internal static class CommonFunctions
    {
        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            DirectoryInfo directoryInfo1 = new DirectoryInfo(sourceDir);
            DirectoryInfo[] directoryInfoArray = directoryInfo1.Exists ? directoryInfo1.GetDirectories() : throw new DirectoryNotFoundException("Source directory not found: " + directoryInfo1.FullName);
            Directory.CreateDirectory(destinationDir);
            foreach (FileInfo file in directoryInfo1.GetFiles())
            {
                string destFileName = Path.Combine(destinationDir, file.Name);
                file.CopyTo(destFileName);
            }
            if (!recursive)
                return;
            foreach (DirectoryInfo directoryInfo2 in directoryInfoArray)
            {
                string destinationDir1 = Path.Combine(destinationDir, directoryInfo2.Name);
                CommonFunctions.CopyDirectory(directoryInfo2.FullName, destinationDir1, true);
            }
        }

        public static void ScrollTo(ListBox l, int index)
        {
            l.Dispatcher.BeginInvoke(new Action(() =>
            {
                l.UpdateLayout();
                l.ScrollIntoView(l.Items[index]);
                l.SelectedIndex = index;
            }));
        }

        public static void ScrollTo(
          ListBox l,
          TextBlock textBlock,
          int index,
          int currentId,
          bool isTitles = false)
        {
            CommonFunctions.ScrollTo(l, index);
            if (isTitles)
                return;
            if (currentId < 0 || currentId == index)
            {
                textBlock.Background = (Brush)AnyConverter.GetBrush("#FFC6C6C6");
                textBlock.Foreground = (Brush)AnyConverter.GetBrush("#7E7E7E");
            }
            else
            {
                textBlock.Background = (Brush)AnyConverter.GetBrush("#FFD4D4D4");
                textBlock.Foreground = (Brush)AnyConverter.GetBrush("#FFFFFFFF");
            }
        }

        public static void ShowPages(int startIndex, int length, ListBox l)
        {
            for (int index = 0; index < startIndex; ++index)
                ((UIElement)l.Items[index]).Visibility = Visibility.Collapsed;
            for (int index = startIndex; index < startIndex + length; ++index)
                ((UIElement)l.Items[index]).Visibility = Visibility.Visible;
            for (int index = startIndex + length; index < l.Items.Count; ++index)
                ((UIElement)l.Items[index]).Visibility = Visibility.Collapsed;
        }

        public static void Search(
          int currentId,
          int currentTitleId,
          string searchBoxText,
          ref int searchedPageId,
          ref string searchingName,
          ref List<int> titlesOrder,
          ListBox list,
          TextBlock textBlock)
        {
            if (currentId < 0)
                return;
            if (searchBoxText.Equals(""))
            {
                CommonFunctions.ScrollTo(list, textBlock, currentId, currentId);
            }
            else
            {
                int num1 = 0;
                if (searchingName.Equals(searchBoxText) && searchedPageId != -1 && ((TextBlock)((ContentControl)list.Items[searchedPageId]).Content).Text.ToLower().Contains(searchingName.ToLower()))
                    num1 = searchedPageId + 1;
                int num2;
                int num3;
                if (currentTitleId != 0)
                {
                    num2 = titlesOrder[currentTitleId];
                    num3 = list.Items.Count;
                    if (currentTitleId + 1 < titlesOrder.Count)
                        num3 = titlesOrder[currentTitleId + 1] - 1;
                    if (num1 < num2 || num1 >= num3)
                        num1 = num2;
                }
                else
                {
                    num2 = 0;
                    num3 = list.Items.Count;
                }
                for (int index = num1; index < num3; ++index)
                {
                    if (((TextBlock)((ContentControl)list.Items[index]).Content).Text.ToLower().Contains(searchBoxText.ToLower()))
                    {
                        searchedPageId = index;
                        searchingName = searchBoxText;
                        CommonFunctions.ScrollTo(list, textBlock, index, currentId);
                        return;
                    }
                }
                if (num1 > num2)
                {
                    for (int index = num2; index < num1; ++index)
                    {
                        if (((TextBlock)((ContentControl)list.Items[index]).Content).Text.ToLower().Contains(searchBoxText.ToLower()))
                        {
                            searchedPageId = index;
                            searchingName = searchBoxText;
                            CommonFunctions.ScrollTo(list, textBlock, index, currentId);
                            return;
                        }
                    }
                }
                searchingName = "";
                searchedPageId = -1;
                CommonFunctions.ScrollTo(list, textBlock, currentId, currentId);
            }
        }

        public static void SaveFileStream(string filePath, Stream stream)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                FileStream destination = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                stream.CopyTo((Stream)destination);
                destination.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Get(int bookId, int userId)
        {
            ServiceBookClient client = AppSettings.GetClient();
            client.Open();
            string[] book = client.GetBook(bookId);
            string str = book[0];
            string path;
            for (path = AppSettings.pathBookCatalog + "\\" + str; Directory.Exists(path); path = AppSettings.pathBookCatalog + "\\" + str)
                str += "_";
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "\\data");
            Directory.CreateDirectory(path + "\\database");
            Task<FileTransferRequest[]> async = client.GetAsync(bookId, userId);
            async.Wait();
            foreach (FileTransferRequest fileTransferRequest in async.Result)
            {
                Directory.CreateDirectory(path + "\\" + fileTransferRequest.Path);
                CommonFunctions.SaveFileStream(path + "\\" + fileTransferRequest.Path + "\\" + fileTransferRequest.FileName, (Stream)new MemoryStream(fileTransferRequest.Content));
            }
            DatabaseContext databaseContext = new DatabaseContext(path + "\\database\\PagesDB.db");
            databaseContext.BookData.First<BookData>().Description = book[1];
            databaseContext.BookData.First<BookData>().UserName = book[2];
            databaseContext.BookData.First<BookData>().CanEdit = Convert.ToInt32(book[3]);
            databaseContext.SaveChanges();
            client.Close();
        }
    }
}
