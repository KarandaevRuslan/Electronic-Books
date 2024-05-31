using ElectronicBooks.Heap.Models;
using ElectronicBooks.Heap.Other;
using ElectronicBooks.Heap.SQLiteEFClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using WK.Libraries.BetterFolderBrowserNS;

namespace ElectronicBooks.Views.Windows
{
    public partial class SettingsWindow : Window
    {
        private string bookCatalog = "";
        private string hostIP = "";
        private AppDBcontext appDBcontext;
        private bool hasBeenSettingsChanged;

        public SettingsWindow()
        {
            this.InitializeComponent();
            this.Closing += new CancelEventHandler(this.SettingsWindow_Closing);
            this.appDBcontext = new AppDBcontext();
            this.HasBeenSettingsChanged = false;
            this.bookCatalogPath.Text = AppSettings.pathBookCatalog;
            this.hostIp.Text = AppSettings.hostIP;
            this.userId.Text = AppSettings.UserId.ToString();
            this.bookCatalog = this.bookCatalogPath.Text;
            this.hostIP = this.hostIp.Text;
        }

        private void SettingsWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!this.HasBeenSettingsChanged)
                return;
            MessageBoxResult messageBoxResult = MessageBox.Show("У вас есть несохраненные данные. Желаете их сохранить?", "Сохраните настройки", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (messageBoxResult <= MessageBoxResult.Cancel)
            {
                if (messageBoxResult != MessageBoxResult.None && messageBoxResult != MessageBoxResult.Cancel)
                    return;
                e.Cancel = true;
            }
            else if (messageBoxResult == MessageBoxResult.Yes)
                this.Save();
        }

        private void Save()
        {
            this.HasBeenSettingsChanged = false;
            this.appDBcontext.Settings.First<Settings>().BookCatalogPath = this.bookCatalogPath.Text;
            this.appDBcontext.Settings.First<Settings>().HostIP = this.hostIp.Text;
            this.appDBcontext.SaveChanges();
            if (!this.bookCatalogPath.Text.Equals(AppSettings.pathBookCatalog))
            {
                try
                {
                    Directory.Move(AppSettings.pathBookCatalog, this.bookCatalogPath.Text);
                }
                catch (Exception ex1)
                {
                    try
                    {
                        CommonFunctions.CopyDirectory(AppSettings.pathBookCatalog, this.bookCatalogPath.Text, true);
                        Directory.Delete(AppSettings.pathBookCatalog, true);
                    }
                    catch (Exception ex2)
                    {
                        int num = (int)MessageBox.Show(ex2.Message);
                    }
                }
            }
            AppSettings.pathBookCatalog = this.appDBcontext.Settings.FirstOrDefault<Settings>().BookCatalogPath;
            AppSettings.hostIP = this.appDBcontext.Settings.FirstOrDefault<Settings>().HostIP;
        }

        private void Back(object sender, MouseButtonEventArgs e) => this.Close();

        private void SaveSettings(object sender, MouseButtonEventArgs e)
        {
            if (!this.HasBeenSettingsChanged)
                return;
            this.Save();
        }

        private void DefaultSettings(object sender, MouseButtonEventArgs e)
        {
            if (this.bookCatalogPath.Text.Equals(AppSettings.defaultPathBookCatalog) && this.hostIp.Text.Equals(AppSettings.hostIP))
                return;
            this.bookCatalog = AppSettings.defaultPathBookCatalog;
            this.hostIP = "___.___.___.___";
            this.HasBeenSettingsChanged = true;
        }

        private void OpenSelectCatalogDialog(object sender, MouseButtonEventArgs e)
        {
            BetterFolderBrowser betterFolderBrowser = new BetterFolderBrowser();
            betterFolderBrowser.Title = "Выберите новое местоположение папки с книгами";
            betterFolderBrowser.ShowDialog();
            if (betterFolderBrowser.SelectedFolder.Equals(""))
                return;
            this.bookCatalog = betterFolderBrowser.SelectedFolder + "\\ElectronicBook";
            if (AppSettings.pathBookCatalog.Equals(this.bookCatalog))
                return;
            this.HasBeenSettingsChanged = true;
        }

        private bool HasBeenSettingsChanged
        {
            get => this.hasBeenSettingsChanged;
            set
            {
                this.hasBeenSettingsChanged = value;
                try
                {
                    if (this.hasBeenSettingsChanged)
                    {
                        this.saveImageButton.Source = (ImageSource)new BitmapImage(new Uri("/ElectronicBooks;component/Resources/Images/saveicon.png", UriKind.Relative));
                        this.bookCatalogPath.Text = this.bookCatalog;
                        this.hostIp.Text = this.hostIP;
                    }
                    else
                        this.saveImageButton.Source = (ImageSource)new BitmapImage(new Uri("/ElectronicBooks;component/Resources/Images/inactivesaveicon.png", UriKind.Relative));
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void OnHostIPChanged(object sender, KeyEventArgs e)
        {
            this.hostIP = this.hostIp.Text;
            this.HasBeenSettingsChanged = true;
        }

    }
}
