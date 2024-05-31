using CefSharp;
using ElectronicBooks.Heap.Behaviors;
using ElectronicBooks.Heap.Models;
using ElectronicBooks.Heap.Other;
using ElectronicBooks.Heap.SQLiteEFClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ElectronicBooks.Views.Windows
{
    public partial class BookViewer : Window
    {
        public string path = "";
        private DatabaseContext context;
        private DatabaseContext unsaved;
        private DatabaseContext saved;
        private bool isControlKeyPressed;
        private int currentId = -1;
        private int currentTitleId;
        private int lastIndexKeyPage = -1;
        private string searchingName = "";
        private int searchedPageId = -1;
        private bool bookHasBeenChanged;
        private bool flagMainWindow;
        private bool flagEditor;
        private List<int> titlesOrder;
        public bool canGoToEditor = true;
        private bool isInfoAboutProgram;
        private double maxZoomLevel = 100.0;
        private double minZoomLevel = 0.5;

        public BookViewer(string path, bool bookHasBeenChanged = false, bool isInfoAboutProgram = false)
        {
            this.InitializeComponent();
            this.PerformInitialSteps(path, bookHasBeenChanged, isInfoAboutProgram);
            List<Pages> pages = this.context.GetPages();
            this.AddAllPages(pages.Count);
            foreach (Pages page in pages)
                this.AddListBoxItemInList(page);
            CommonFunctions.ScrollTo(this.titles, this.toOpenPage, 0, this.CurrentId, true);
            this.UpdateTitlesOrder();
            if (this.list.Items.Count > 0)
            {
                this.CurrentId = 0;
                this.ActionSelect();
            }
            else
                this.CurrentId = -1;
        }

        private void BookViewer_Closing(object sender, CancelEventArgs e)
        {
            if (this.BookHasBeenChanged && !this.flagEditor)
            {
                switch (MessageBox.Show("У вас есть несохраненные данные. Желаете их сохранить?", "Сохраните книгу", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.None:
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        this.flagMainWindow = this.flagEditor = false;
                        return;
                    case MessageBoxResult.Yes:
                        this.Save();
                        break;
                }
            }
            if (File.Exists(this.path + "\\database\\UnsavedPagesDB.db") && !this.flagEditor && this.unsaved.Pages.Count<Pages>() != 0)
            {
                this.unsaved.Pages.RemoveRange((IEnumerable<Pages>)this.unsaved.Pages);
                this.unsaved.SaveChanges();
            }
            if (this.flagMainWindow)
            {
                new MainWindow().Show();
            }
            else
            {
                if (!this.flagEditor)
                    return;
                new BookEditor(this.path, this.BookHasBeenChanged).Show();
            }
        }

        private void ToMainWindow(object sender, MouseButtonEventArgs e)
        {
            if (!this.isInfoAboutProgram)
                this.flagMainWindow = true;
            this.Close();
        }

        private void ToEditor(object sender, MouseButtonEventArgs e)
        {
            if (this.saved.BookData.First<BookData>().CanEdit == 0)
            {
                int num = (int)MessageBox.Show("Эту книгу запрещено редактировать.", "Нельзя редактировать", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                if (!this.saved.BookData.FirstOrDefault<BookData>().PasswordText.Equals("") && this.saved.BookData.FirstOrDefault<BookData>().WasActivated == 1)
                {
                    this.canGoToEditor = false;
                    EnterPasswordWindow enterPasswordWindow = new EnterPasswordWindow(this.path);
                    enterPasswordWindow.Owner = (Window)this;
                    enterPasswordWindow.ShowDialog();
                    if (!this.canGoToEditor)
                        return;
                }
                this.flagEditor = true;
                this.Close();
            }
        }

        private void UpdateTitlesOrder()
        {
            this.titlesOrder.Clear();
            this.titlesOrder.Add(0);
            this.titlesOrder.AddRange((IEnumerable<int>)this.context.Pages.OrderBy<Pages, int>((Expression<Func<Pages, int>>)(x => x.PageOrder)).Where<Pages>((Expression<Func<Pages, bool>>)(x => x.IsTitle == 1)).Select<Pages, int>((Expression<Func<Pages, int>>)(x => x.PageOrder)).ToList<int>());
        }

        private void ActionSelect()
        {
            if (this.currentId < 0)
                return;
            Pages page = this.context.GetPage(this.CurrentId + 1);
            this.browser.Address = page.AbsoluteLink != null || page.RelativeLink != null ? (page.AbsoluteLink != null ? page.AbsoluteLink : "file:///" + this.path.Replace("\\", "/") + "/data/" + page.RelativeLink) : "null";
            CommonFunctions.ScrollTo(this.list, this.toOpenPage, this.CurrentId, this.CurrentId);
        }

        private void AddAllPages(int pagesCount)
        {
            if (this.lastIndexKeyPage != -1)
                return;
            ListBoxItem listBoxItem = this.GetListBoxItem("Все страницы", 120);
            listBoxItem.MouseLeftButtonUp += (MouseButtonEventHandler)((sender, e) =>
            {
                CommonFunctions.ScrollTo(this.titles, this.toOpenPage, this.titles.Items.IndexOf(sender), this.CurrentId, true);
                CommonFunctions.ShowPages(0, pagesCount, this.list);
                this.currentTitleId = 0;
            });
            this.lastIndexKeyPage = 0;
            this.titles.Items.Add((object)listBoxItem);
        }

        private void AddListBoxItemInList(Pages page)
        {
            if (AnyConverter.IntToBool(page.IsTitle))
            {
                ++this.lastIndexKeyPage;
                int ind = this.lastIndexKeyPage;
                int way = this.WayToNextTitle(page.PageOrder);
                ListBoxItem listBoxItem = this.GetListBoxItem(page, 120, false);
                listBoxItem.MouseLeftButtonUp += (MouseButtonEventHandler)((sender, e) =>
                {
                    CommonFunctions.ScrollTo(this.titles, this.toOpenPage, this.titles.Items.IndexOf(sender), this.CurrentId, true);
                    CommonFunctions.ShowPages(page.PageOrder, way, this.list);
                    this.CurrentId = page.PageOrder - 1;
                    this.currentTitleId = ind;
                    this.ActionSelect();
                });
                this.titles.Items.Add((object)listBoxItem);
            }
            ListBoxItem listBoxItem1 = this.GetListBoxItem(page, 120, AnyConverter.IntToBool(page.IsTitle));
            int lastIndexKeyPage = this.lastIndexKeyPage;
            listBoxItem1.MouseLeftButtonUp += (MouseButtonEventHandler)((sender, e) =>
            {
                this.CurrentId = page.PageOrder - 1;
                this.ActionSelect();
            });
            this.list.Items.Add((object)listBoxItem1);
        }

        private void Search(object sender, KeyEventArgs e)
        {
            CommonFunctions.Search(this.CurrentId, this.currentTitleId, this.searchBox.Text, ref this.searchedPageId, ref this.searchingName, ref this.titlesOrder, this.list, this.toOpenPage);
        }

        private void Save()
        {
            this.BookHasBeenChanged = false;
            try
            {
                this.unsaved.SaveChanges();
                this.context.SaveChanges();
                this.saved.SaveChanges();
                this.saved.Pages.RemoveRange((IEnumerable<Pages>)this.saved.Pages);
                foreach (Pages page in (IEnumerable<Pages>)this.unsaved.Pages)
                {
                    Pages entity = (Pages)page.Clone();
                    entity.Id = entity.PageOrder;
                    this.saved.Pages.Add(entity);
                }
                this.unsaved.Pages.RemoveRange((IEnumerable<Pages>)this.unsaved.Pages);
                this.unsaved.SaveChanges();
                this.saved.SaveChanges();
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.Message, "Непредвиденная ошибка", MessageBoxButton.OK, MessageBoxImage.Hand) == MessageBoxResult.None)
                    ;
            }
        }

        private void SaveFile(object sender, MouseButtonEventArgs e)
        {
            if (!this.BookHasBeenChanged)
                return;
            this.Save();
        }

        private void ToOpenedPage(object sender, MouseButtonEventArgs e)
        {
            if (this.CurrentId < 0)
                return;
            CommonFunctions.ScrollTo(this.list, this.toOpenPage, this.CurrentId, this.CurrentId);
            CommonFunctions.ScrollTo(this.titles, this.toOpenPage, this.currentTitleId, this.CurrentId, true);
        }

        private ListBoxItem GetListBoxItem(Pages page, int buttonWidth, bool isTitle)
        {
            ListBoxItem listBoxItem = new ListBoxItem();
            TextBlock textBlock = new TextBlock()
            {
                Text = page.Title,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new FontFamily("Times New Roman"),
                TextAlignment = TextAlignment.Center,
                Foreground = (Brush)AnyConverter.GetBrush(page.Color)
            };
            if (isTitle)
            {
                textBlock.TextDecorations = TextDecorations.Underline;
                textBlock.FontWeight = FontWeights.Bold;
                textBlock.FontSize = 16.0;
            }
            else
            {
                textBlock.FontWeight = FontWeights.Normal;
                textBlock.FontSize = 15.0;
            }
            listBoxItem.Content = (object)textBlock;
            listBoxItem.Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
            listBoxItem.BorderBrush = (Brush)null;
            listBoxItem.MinWidth = (double)buttonWidth;
            listBoxItem.MaxWidth = (double)buttonWidth;
            return listBoxItem;
        }

        private ListBoxItem GetListBoxItem(string title, int buttonWidth)
        {
            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Content = (object)new TextBlock()
            {
                Text = title,
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

        private void PerformInitialSteps(string path, bool bookHasBeenChanged, bool isInfoAboutProgram)
        {
            this.Closing += new CancelEventHandler(this.BookViewer_Closing);
            this.isInfoAboutProgram = isInfoAboutProgram;
            this.path = path;
            if (this.isInfoAboutProgram)
            {
                this.saveImageButton.Visibility = Visibility.Collapsed;
                this.toEditorImageButton.Visibility = Visibility.Collapsed;
                this.infoImageButton.Visibility = Visibility.Collapsed;
                this.DescriptionImageButton.Visibility = Visibility.Collapsed;
                this.backImageButton.ToolTip = (object)"Назад";
            }
            this.unsaved = new DatabaseContext(this.path + "\\database\\UnsavedPagesDB.db");
            this.saved = new DatabaseContext(this.path + "\\database\\PagesDB.db");
            this.BookHasBeenChanged = bookHasBeenChanged;
            this.browser.DownloadHandler = (IDownloadHandler)new DownloadHandler();
            this.browser.LifeSpanHandler = (ILifeSpanHandler)new CustomLifeSpanHandler();
            this.browser.MenuHandler = (IContextMenuHandler)new CustomMenuHandler();
            string[] strArray = this.path.Split('\\');
            this.NameBook.Text = strArray[strArray.Length - 1];
            this.titlesOrder = new List<int>();
        }

        private int WayToNextTitle(int curentOrder)
        {
            return this.context.GetNextPagesTitleOrder(curentOrder) - curentOrder - 1;
        }

        private int CurrentId
        {
            get => this.currentId;
            set
            {
                if (this.list.Items.Count == 0)
                {
                    this.browser.Address = "null";
                    this.list.SelectedIndex = -1;
                    this.currentId = -1;
                    this.toOpenPage.Background = (Brush)AnyConverter.GetBrush("#FFC6C6C6");
                    this.toOpenPage.Foreground = (Brush)AnyConverter.GetBrush("#7E7E7E");
                }
                else
                    this.currentId = value;
            }
        }

        private bool BookHasBeenChanged
        {
            get => this.bookHasBeenChanged;
            set
            {
                this.bookHasBeenChanged = value;
                try
                {
                    if (this.bookHasBeenChanged)
                    {
                        this.saveImageButton.Source = (ImageSource)new BitmapImage(new Uri("/ElectronicBooks;component/Resources/Images/saveicon.png", UriKind.Relative));
                        this.context = this.unsaved;
                    }
                    else
                    {
                        this.saveImageButton.Source = (ImageSource)new BitmapImage(new Uri("/ElectronicBooks;component/Resources/Images/inactivesaveicon.png", UriKind.Relative));
                        this.context = this.saved;
                    }
                }
                catch (Exception ex)
                {
                }
            }
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

        private void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.RightCtrl && e.Key != Key.LeftCtrl)
                return;
            this.isControlKeyPressed = false;
        }

        private void OnKPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.RightCtrl && e.Key != Key.LeftCtrl)
                return;
            this.isControlKeyPressed = true;
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!this.isControlKeyPressed)
                return;
            if (e.Delta > 0 && this.browser.ZoomLevel <= this.maxZoomLevel)
            {
                this.browser.ZoomInCommand.Execute((object)null);
            }
            else
            {
                if (e.Delta >= 0 || this.browser.ZoomLevel < this.minZoomLevel)
                    return;
                this.browser.ZoomOutCommand.Execute((object)null);
            }
        }

        private void OnDescription(object sender, MouseButtonEventArgs e)
        {
            if (this.BookHasBeenChanged)
            {
                switch (MessageBox.Show("Чтобы открыть описание книги, нужно сначала сохранить книгу. Желаете ее сохранить?", "Сохраните книгу", MessageBoxButton.YesNo, MessageBoxImage.Asterisk))
                {
                    case MessageBoxResult.None:
                        return;
                    case MessageBoxResult.Yes:
                        this.Save();
                        break;
                    case MessageBoxResult.No:
                        return;
                }
            }
            new BookDescription(this.context, this.NameBook.Text).ShowDialog();
        }
    }
}
