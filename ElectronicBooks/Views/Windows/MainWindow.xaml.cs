using ElectronicBooks.BookReference;
using ElectronicBooks.Heap.Models;
using ElectronicBooks.Heap.Other;
using ElectronicBooks.Heap.SQLiteEFClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WK.Libraries.BetterFolderBrowserNS;

namespace ElectronicBooks.Views.Windows
{
    public partial class MainWindow : Window
    {
        public string currentName = "";
        public bool canGoToEditor = true;
        public int currentMode;
        public bool canGoToRegistration;
        private AppDBcontext appDBcontext;

        public MainWindow()
        {
            this.InitializeComponent();
            this.appDBcontext = new AppDBcontext();
            if (this.appDBcontext.Settings.Count<Settings>() == 0)
            {
                this.appDBcontext.Settings.Add(new Settings()
                {
                    Id = 1,
                    BookCatalogPath = AppSettings.defaultPathBookCatalog,
                    UserId = AppSettings.UserId,
                    HostIP = "___.___.___.___"
                });
                this.appDBcontext.SaveChanges();
            }
            AppSettings.UserId = this.appDBcontext.Settings.FirstOrDefault<Settings>().UserId;
            AppSettings.pathBookCatalog = this.appDBcontext.Settings.FirstOrDefault<Settings>().BookCatalogPath;
            AppSettings.hostIP = this.appDBcontext.Settings.FirstOrDefault<Settings>().HostIP;
            this.list.ToolTip = (object)("Папка с книгами: " + AppSettings.pathBookCatalog);
            if (!Directory.Exists(AppSettings.pathBookCatalog))
                Directory.CreateDirectory(AppSettings.pathBookCatalog);
            this.FillListBox();
            this.UpdateUserNameText();
        }

        private void UpdateUserNameText()
        {
            if (AppSettings.UserId == 0)
                this.UserName.Text = "Пользователь: Гость";
            else
                new Thread((ThreadStart)(() =>
                {
                    try
                    {
                        ServiceBookClient client = AppSettings.GetClient();
                        client.Open();
                        this.Dispatcher.Invoke((System.Action)(() => this.UserName.Text = "Пользователь: " + client.GetName(AppSettings.UserId)));
                        client.Close();
                    }
                    catch (Exception ex)
                    {
                        this.Dispatcher.Invoke((System.Action)(() => this.UserName.Text = "Не удалось войти в аккаунт"));
                    }
                })).Start();
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
            newItem.MinWidth = 370.0;
            newItem.MaxWidth = 370.0;
            newItem.Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
            newItem.MouseLeftButtonUp += (MouseButtonEventHandler)((sender, e) =>
            {
                string str1 = AppSettings.pathBookCatalog + "\\" + ((TextBlock)((ContentControl)sender).Content).Text;
                this.ClearUnsavedDB(str1);
                this.InitializeBookData(new DatabaseContext(str1 + "\\database\\PagesDB.db"));
                new BookViewer(str1).Show();
                this.SetParamsNull();
                this.Close();
            });
            newItem.MouseRightButtonUp += (MouseButtonEventHandler)((sender, e) => this.Action((ListBoxItem)sender));
            this.list.Items.Add((object)newItem);
        }

        private void CreateBook(object sender, MouseButtonEventArgs e)
        {
            try
            {
                BookName bookName = new BookName();
                bookName.Owner = (Window)this;
                bookName.ShowDialog();
                if (this.currentName.Equals(""))
                    return;
                this.NewBookActions();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void NewBookActions()
        {
            this.FillListBox();
            CommonFunctions.ScrollTo(this.list, this.IndexOf(this.currentName, this.list));
            this.currentName = "";
        }

        private void FillListBox()
        {
            string pathBookCatalog = AppSettings.pathBookCatalog;
            if (!Directory.Exists(pathBookCatalog))
                Directory.CreateDirectory(pathBookCatalog);
            string[] directories = Directory.GetDirectories(pathBookCatalog);
            this.list.Items.Clear();
            foreach (string str in directories)
                this.AddListBoxItemInList(str);
        }

        private void InitializeBookData(DatabaseContext d)
        {
            if (d.BookData.Count<BookData>() != 0)
                return;
            string str;
            if (AppSettings.UserId == 0)
            {
                str = "Гость";
            }
            else
            {
                ServiceBookClient client = AppSettings.GetClient();
                try
                {
                    client.Open();
                    str = client.GetName(AppSettings.UserId);
                    client.Close();
                }
                catch (Exception ex)
                {
                    str = "Гость";
                }
            }
            d.BookData.Add(new BookData()
            {
                UserName = str,
                Description = "",
                CanEdit = 1,
                PasswordText = "",
                WasActivated = 0
            });
            d.SaveChanges();
        }

        private void Action(ListBoxItem b)
        {
            string str = AppSettings.pathBookCatalog + "\\" + ((TextBlock)b.Content).Text;
            ModeSelection modeSelection = new ModeSelection();
            modeSelection.Owner = (Window)this;
            modeSelection.ShowDialog();
            switch (this.currentMode)
            {
                case 1:
                    DatabaseContext d = new DatabaseContext(str + "\\database\\PagesDB.db");
                    this.InitializeBookData(d);
                    if (d.BookData.First<BookData>().CanEdit == 0)
                    {
                        int num = (int)MessageBox.Show("Эту книгу запрещено редактировать.", "Нельзя редактировать", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        this.SetParamsNull();
                        return;
                    }
                    if (!d.BookData.FirstOrDefault<BookData>().PasswordText.Equals("") && d.BookData.FirstOrDefault<BookData>().WasActivated == 1)
                    {
                        this.canGoToEditor = false;
                        EnterPasswordWindow enterPasswordWindow = new EnterPasswordWindow(str);
                        enterPasswordWindow.Owner = (Window)this;
                        enterPasswordWindow.ShowDialog();
                        if (!this.canGoToEditor)
                        {
                            this.SetParamsNull();
                            return;
                        }
                    }
                    this.ClearUnsavedDB(str);
                    new BookEditor(str).Show();
                    break;
                case 2:
                    this.ClearUnsavedDB(str);
                    this.InitializeBookData(new DatabaseContext(str + "\\database\\PagesDB.db"));
                    new BookViewer(str).Show();
                    break;
                case 3:
                    this.OpenExportDialog(str);
                    this.SetParamsNull();
                    return;
                case 4:
                    this.DeleteBook();
                    return;
                case 5:
                    DatabaseContext databaseContext = new DatabaseContext(str + "\\database\\PagesDB.db");
                    this.InitializeBookData(databaseContext);
                    new BookDescription(databaseContext, ((TextBlock)b.Content).Text).ShowDialog();
                    this.SetParamsNull();
                    return;
                default:
                    this.SetParamsNull();
                    return;
            }
            this.SetParamsNull();
            this.Close();
        }

        private void ClearUnsavedDB(string pathBookFolder)
        {
            DatabaseContext databaseContext = new DatabaseContext(pathBookFolder + "\\database\\UnsavedPagesDB.db");
            if (File.Exists(pathBookFolder + "\\database\\UnsavedPagesDB.db") && databaseContext.Pages.Count<Pages>() != 0)
                databaseContext.Pages.RemoveRange((IEnumerable<Pages>)databaseContext.Pages);
            databaseContext.SaveChanges();
        }

        private void DeleteBook()
        {
            string text;
            try
            {
                text = ((TextBlock)((ContentControl)this.list.Items[this.list.SelectedIndex]).Content).Text;
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
                this.SetParamsNull();
                return;
            }
            switch (MessageBox.Show("Удаление книги \"" + text + "\" будет безвозвратным. Желаете удалить книгу?", "Удаление книги", MessageBoxButton.YesNo, MessageBoxImage.Exclamation))
            {
                case MessageBoxResult.Yes:
                    string path = AppSettings.pathBookCatalog + "\\" + text;
                    try
                    {
                        Directory.Delete(path, true);
                        this.list.Items.RemoveAt(this.list.SelectedIndex);
                        break;
                    }
                    catch (Exception ex)
                    {
                        int num = (int)MessageBox.Show(ex.Message, "Ошибка при попытке удаления книги", MessageBoxButton.OK, MessageBoxImage.Hand);
                        break;
                    }
            }
            this.SetParamsNull();
        }

        private void SetParamsNull()
        {
            this.currentName = "";
            this.currentMode = 0;
            this.list.SelectedIndex = -1;
        }

        private int IndexOf(string text, ListBox l)
        {
            int num = 0;
            foreach (ContentControl contentControl in (IEnumerable)l.Items)
            {
                if (((TextBlock)contentControl.Content).Text.Equals(text))
                    return num;
                ++num;
            }
            return -1;
        }

        private void ImportBook(object sender, MouseButtonEventArgs e)
        {
            this.OpenImportDialog();
            if (this.currentName.Equals(""))
                return;
            this.NewBookActions();
        }

        private void OpenImportDialog()
        {
            BetterFolderBrowser betterFolderBrowser = new BetterFolderBrowser();
            betterFolderBrowser.Title = "Выберите папку книги";
            int num = (int)betterFolderBrowser.ShowDialog();
            if (betterFolderBrowser.SelectedFolder.Equals(""))
                return;
            this.SubImport(betterFolderBrowser.SelectedFolder);
        }

        private void SubImport(string bookPath)
        {
            bookPath = AnyConverter.GetShortDirectoryPath(bookPath);
            if (bookPath.Equals("") || bookPath == null)
            {
                int num1 = (int)MessageBox.Show("Ошибка ввода.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            else if (!Directory.Exists(bookPath + "\\data") || !Directory.Exists(bookPath + "\\database"))
            {
                int num2 = (int)MessageBox.Show("Выбранная папка не является книгой.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            else
            {
                string directoryName = AnyConverter.GetDirectoryName(bookPath);
                string destinationDir = AppSettings.pathBookCatalog + "\\" + directoryName;
                try
                {
                    if (!bookPath.Equals(destinationDir))
                        CommonFunctions.CopyDirectory(bookPath, destinationDir, true);
                    this.currentName = directoryName;
                }
                catch (Exception ex)
                {
                    int num3 = (int)MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
                }
            }
        }

        private void OpenExportDialog(string bookPath)
        {
            BetterFolderBrowser betterFolderBrowser = new BetterFolderBrowser();
            betterFolderBrowser.Title = "Выберите папку, в которую будет экспортирована книга";
            int num = (int)betterFolderBrowser.ShowDialog();
            if (betterFolderBrowser.SelectedFolder.Equals(""))
                return;
            this.SubExport(bookPath, betterFolderBrowser.SelectedFolder);
        }

        private void SubExport(string bookPath, string expPath)
        {
            expPath = AnyConverter.GetShortDirectoryPath(expPath);
            if (expPath.Equals("") || expPath == null)
            {
                int num1 = (int)MessageBox.Show("Ошибка ввода.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            else if (expPath.Contains(AppSettings.pathBookCatalog))
            {
                int num2 = (int)MessageBox.Show("Папка книги не может быть экспортирована в папку с каталогом книг.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            else
            {
                string directoryName = AnyConverter.GetDirectoryName(bookPath);
                expPath = expPath + "\\" + directoryName;
                try
                {
                    CommonFunctions.CopyDirectory(bookPath, expPath, true);
                }
                catch (Exception ex)
                {
                    int num3 = (int)MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
                }
            }
        }

        private void OnSettings(object sender, MouseButtonEventArgs e)
        {
            new SettingsWindow().ShowDialog();
            this.list.ToolTip = (object)("Папка с книгами: " + AppSettings.pathBookCatalog);
        }

        private void OnInfo(object sender, MouseButtonEventArgs e)
        {
            try
            {
                new BookViewer(AppSettings.AppDirectoryPath() + "\\Руководство пользователя", isInfoAboutProgram: true).ShowDialog();
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message);
            }
        }

        private void OnAccount(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ServiceBookClient client = AppSettings.GetClient();
                client.Open();
                client.GetName(AppSettings.UserId);
                client.Close();
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show("Нет подключения к серверу. Попробуйте подключиться позже.", "Ошибка соединения", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            if (AppSettings.UserId == 0)
            {
                Authentication authenticaton = new Authentication();
                authenticaton.Owner = (Window)this;
                authenticaton.ShowDialog();
                if (this.canGoToRegistration)
                {
                    this.canGoToRegistration = false;
                    Registration registration = new Registration();
                    registration.Owner = (Window)this;
                    registration.ShowDialog();
                }
                if (AppSettings.UserId != 0)
                    this.UpdateUserNameText();
            }
            if (AppSettings.UserId == 0)
                return;
            new Account(AppSettings.UserId).Show();
            this.Close();
        }

        private void OnStorage(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ServiceBookClient client = AppSettings.GetClient();
                client.Open();
                client.GetName(AppSettings.UserId);
                client.Close();
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show("Нет подключения к серверу. Попробуйте подключиться позже.", "Ошибка соединения", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            if (AppSettings.UserId != 0)
            {
                new BookSearch(AppSettings.UserId).Show();
                this.Close();
            }
            else
            {
                int num1 = (int)MessageBox.Show("Вы не вошли в аккаунт. Войдите в аккаунт или зарегистрируйте его.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }
    }
}
