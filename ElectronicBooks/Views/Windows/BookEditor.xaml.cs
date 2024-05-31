using CefSharp;
using ElectronicBooks.Heap.Behaviors;
using ElectronicBooks.Heap.Models;
using ElectronicBooks.Heap.Other;
using ElectronicBooks.Heap.SQLiteEFClasses;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
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
    public partial class BookEditor : Window
    {
        public string path = "";
        private DatabaseContext context;
        private DatabaseContext unsaved;
        private DatabaseContext saved;
        private int currentId = -1;
        private int currentTitleId;
        private int hoveringListBox = -1;
        private Pages copiedPage;
        private int hoveringIdList = -1;
        private List<Pages> copiedPages;
        private int hoveringIdTitles = -1;
        private bool flagMainWindow;
        private bool flagViewer;
        private string searchingName = "";
        private int searchedPageId = -1;
        private bool bookHasBeenChanged;
        private int currentTypeLink;
        private int generatedId;
        private bool isControlKeyPressed;
        private double maxZoomLevel = 100.0;
        private double minZoomLevel = 0.5;
        private List<int> titlesOrder;

        public BookEditor(string path, bool bookHasBeenChanged = false)
        {
            try
            {
                this.InitializeComponent();
                this.PerformInitialSteps(path, bookHasBeenChanged);
                List<Pages> pages1 = this.context.GetPages();
                this.AddAllPages();
                foreach (Pages pages2 in pages1)
                    this.AddListBoxItemInList((Pages)pages2.Clone());
                CommonFunctions.ScrollTo(this.titles, this.toOpenPage, 0, this.CurrentId, true);
                if (this.list.Items.Count > 0)
                {
                    this.CurrentId = 0;
                    this.ActionSelect();
                }
                else
                    this.CurrentId = -1;
            }
            catch (Exception ex)
            {
                int num = (int)System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void BookEditor_Closing(object sender, CancelEventArgs e)
        {
            if (this.BookHasBeenChanged && !this.flagViewer)
            {
                switch (System.Windows.MessageBox.Show("У вас есть несохраненные данные. Желаете их сохранить?", "Сохраните книгу", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.None:
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        this.flagMainWindow = this.flagViewer = false;
                        return;
                    case MessageBoxResult.Yes:
                        this.Save();
                        break;
                }
            }
            if (File.Exists(this.path + "\\database\\UnsavedPagesDB.db") && !this.flagViewer)
            {
                if (this.unsaved.Pages.Select<Pages, Pages>((Expression<Func<Pages, Pages>>)(p => p)).Count<Pages>() != 0)
                {
                    this.unsaved.Pages.RemoveRange((IEnumerable<Pages>)this.unsaved.Pages);
                    this.unsaved.SaveChanges();
                }
            }
            if (this.flagMainWindow)
            {
                new MainWindow().Show();
            }
            else
            {
                if (!this.flagViewer)
                    return;
                new BookViewer(this.path, this.BookHasBeenChanged).Show();
            }
        }

        private void ToMainWindow(object sender, MouseButtonEventArgs e)
        {
            this.flagMainWindow = true;
            this.Close();
        }

        private void ToViewer(object sender, MouseButtonEventArgs e)
        {
            this.flagViewer = true;
            this.Close();
        }

        private void SelectTypeLink(object sender, RoutedEventArgs e)
        {
            if (this.currentId < 0)
                return;
            this.CurrentTypeLink = this.typeLink.Items.IndexOf(sender);
        }

        private void InsertPage(Pages page)
        {
            this.BookHasBeenChanged = true;
            Pages entity = (Pages)page.Clone();
            int pOrder = entity.PageOrder;
            DbSet<Pages> pages1 = this.context.Pages;
            Expression<Func<Pages, bool>> predicate = (Expression<Func<Pages, bool>>)(x => x.PageOrder >= pOrder);
            foreach (Pages pages2 in pages1.Where<Pages>(predicate).ToList<Pages>())
                ++pages2.PageOrder;
            this.context.SaveChanges();
            entity.Id = this.GetGeneratedId();
            try
            {
                this.context.Pages.Add(entity);
                this.context.SaveChanges();
                this.UpdateTitlesOrder();
            }
            catch (Exception ex)
            {
            }
        }

        private void DeletePage(int pOrder)
        {
            this.BookHasBeenChanged = true;
            this.context.Pages.RemoveRange((IEnumerable<Pages>)this.context.Pages.Where<Pages>((Expression<Func<Pages, bool>>)(x => x.PageOrder == pOrder)));
            this.context.SaveChanges();
            DbSet<Pages> pages1 = this.context.Pages;
            Expression<Func<Pages, bool>> predicate = (Expression<Func<Pages, bool>>)(x => x.PageOrder > pOrder);
            foreach (Pages pages2 in pages1.Where<Pages>(predicate).ToList<Pages>())
                --pages2.PageOrder;
            this.context.SaveChanges();
            this.UpdateTitlesOrder();
        }

        private void Action(ListBoxItem item)
        {
            this.CurrentId = this.list.Items.IndexOf((object)item);
            this.ActionSelect();
        }

        private void ActionSelect()
        {
            try
            {
                if (this.CurrentId < 0)
                    return;
                Pages page = this.context.GetPage(this.CurrentId + 1);
                this.title.Text = page.Title;
                this.color.Text = page.Color;
                this.colorPicker.SelectedColor = new System.Windows.Media.Color?(AnyConverter.GetBrush(this.color.Text).Color);
                this.isTitle.IsChecked = new bool?(AnyConverter.IntToBool(page.IsTitle));
                if (page.AbsoluteLink == null && page.RelativeLink == null)
                {
                    this.link.Text = "null";
                    this.CurrentTypeLink = 2;
                }
                else if (page.AbsoluteLink == null)
                {
                    this.link.Text = "file:///" + this.path.Replace("\\", "/") + "/data/" + page.RelativeLink;
                    this.CurrentTypeLink = 1;
                }
                else
                {
                    this.link.Text = page.AbsoluteLink;
                    this.CurrentTypeLink = 0;
                }
                this.browser.Address = this.link.Text;
                CommonFunctions.ScrollTo(this.list, this.toOpenPage, this.CurrentId, this.CurrentId);
            }
            catch (Exception ex)
            {
                int num = (int)System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void ActionAdd(int currentElement)
        {
            Pages page = new Pages();
            this.CurrentId = this.currentTitleId != 0 ? this.context.GetNextPagesTitleOrder(this.titlesOrder[this.currentTitleId]) - 1 : this.list.Items.Count;
            switch (currentElement)
            {
                case 0:
                    this.link.Text = "null";
                    this.title.Text = "Введите название страницы (глава)";
                    page.AbsoluteLink = (string)null;
                    page.RelativeLink = (string)null;
                    this.CurrentTypeLink = 2;
                    page.IsTitle = 1;
                    break;
                case 1:
                    this.link.Text = "file:///" + this.path.Replace("\\", "/") + "/data/";
                    this.title.Text = "Введите название страницы (документ)";
                    page.AbsoluteLink = (string)null;
                    page.RelativeLink = "";
                    this.CurrentTypeLink = 1;
                    page.IsTitle = 0;
                    break;
                case 2:
                    this.link.Text = "https://www.google.com/";
                    this.title.Text = "Введите название страницы (сайт)";
                    break;
                case 3:
                    this.link.Text = "https://convertio.co/ru";
                    this.title.Text = "Введите название страницы (конвертер документов)";
                    break;
                case 4:
                    this.link.Text = "https://www.desmos.com/calculator?lang=ru";
                    this.title.Text = "Введите название страницы (график)";
                    break;
                case 5:
                    this.link.Text = "https://translate.google.com/?hl=ru&sl=auto&tl=ru&op=websites";
                    this.title.Text = "Введите название страницы (Google перевод)";
                    break;
                case 6:
                    this.link.Text = "https://docs.google.com/forms";
                    this.title.Text = "Введите название страницы (Google форма)";
                    break;
                case 7:
                    this.link.Text = "https://drive.google.com/drive/u/0/my-drive";
                    this.title.Text = "Введите название страницы (Google диск)";
                    break;
            }
            if (currentElement != 1 && currentElement != 0)
            {
                page.RelativeLink = (string)null;
                page.AbsoluteLink = this.link.Text;
                this.CurrentTypeLink = 0;
                page.IsTitle = 0;
            }
            page.Color = this.color.Text = "#7E7E7E";
            this.colorPicker.SelectedColor = new System.Windows.Media.Color?(AnyConverter.GetBrush(this.color.Text).Color);
            page.Title = this.title.Text;
            this.isTitle.IsChecked = new bool?(AnyConverter.IntToBool(page.IsTitle));
            page.PageOrder = this.CurrentId + 1;
            this.browser.Address = this.link.Text;
            this.list.Items.Insert(this.CurrentId, (object)this.GetListBoxItemWithCommands(page, 140, AnyConverter.IntToBool(page.IsTitle)));
            this.InsertPage(page);
            if (currentElement == 0)
            {
                this.UpdateTitles();
                this.CommandOfTitlesItem(this.titlesOrder.IndexOf(page.PageOrder));
            }
            CommonFunctions.ScrollTo(this.list, this.toOpenPage, this.CurrentId, this.CurrentId);
        }

        private void ActionDelete()
        {
            int num = 0;
            if (AnyConverter.IntToBool(this.context.GetPage(this.CurrentId + 1).IsTitle))
                num = 1;
            this.DeletePage(this.CurrentId + 1);
            this.list.Items.RemoveAt(this.CurrentId);
            if (num == 1)
            {
                this.UpdateTitles();
                this.SetAllPages();
            }
            this.CurrentId = -1;
        }

        private void SelectCurrentPageLink(object sender, RoutedEventArgs e)
        {
            if (this.CurrentId == -1)
                return;
            Pages page = this.context.GetPage(this.CurrentId + 1);
            if (page.AbsoluteLink == null && page.RelativeLink == null)
                return;
            this.link.Text = this.browser.Address;
            this.SubSavePage();
        }

        private void SavePage(object sender, RoutedEventArgs e)
        {
            if (this.CurrentId == -1)
                return;
            this.SubSavePage();
        }

        private void SavePage(object sender, KeyEventArgs e)
        {
            if (this.CurrentId == -1 || e.Key != Key.Return)
                return;
            this.SubSavePage();
        }

        private void SubSavePage()
        {
            if (this.currentId == -1)
            {
                int num1 = (int)System.Windows.MessageBox.Show("В книге нет страниц");
            }
            else if (this.title.Text.Equals("") && this.link.Text.Equals("") || this.color.Text.Equals(""))
            {
                int num2 = (int)System.Windows.MessageBox.Show("Заполните пустые поля");
            }
            else
            {
                this.BookHasBeenChanged = true;
                Pages page = this.context.GetPage(this.currentId + 1);
                int isTitle = page.IsTitle;
                string color = page.Color;
                string title = page.Title;
                page.Title = this.title.Text;
                page.Color = this.color.Text;
                this.colorPicker.SelectedColor = new System.Windows.Media.Color?(AnyConverter.GetBrush(this.color.Text).Color);
                page.IsTitle = AnyConverter.BoolToInt(this.isTitle.IsChecked.Value);
                string str1 = AnyConverter.EncodeAndDecodeUrl(this.link.Text);
                AnyConverter.IsFile(str1);
                string p = "file:///" + this.path.Replace('\\', '/') + "/data";
                if (str1.IndexOf(p) == 0 && this.IsNotDatabase(str1, p))
                {
                    this.CurrentTypeLink = 1;
                    AnyConverter.ConvertionVideoToWebm(str1.Substring(8));
                }
                else if (str1.IndexOf("file:///") == 0)
                {
                    this.CurrentTypeLink = 0;
                    AnyConverter.ConvertionVideoToWebm(str1.Substring(8));
                }
                switch (this.CurrentTypeLink)
                {
                    case 0:
                        this.browser.Address = this.link.Text;
                        page.AbsoluteLink = this.link.Text;
                        page.RelativeLink = (string)null;
                        break;
                    case 1:
                        string directoryName = AnyConverter.GetDirectoryName(AppSettings.pathBookCatalog);
                        string str2 = this.link.Text.Substring(this.link.Text.IndexOf(directoryName) + directoryName.Length);
                        string str3 = str2.Substring(str2.IndexOf("data/") + 5);
                        this.browser.Address = this.link.Text;
                        page.AbsoluteLink = (string)null;
                        page.RelativeLink = str3;
                        break;
                    case 2:
                        page.AbsoluteLink = (string)null;
                        page.RelativeLink = (string)null;
                        this.link.Text = "null";
                        this.browser.Address = "null";
                        break;
                }
                this.context.Pages.RemoveRange((IEnumerable<Pages>)this.context.Pages.Where<Pages>((Expression<Func<Pages, bool>>)(x => x.PageOrder == this.currentId + 1)));
                this.context.SaveChanges();
                page.Id = this.GetGeneratedId();
                this.context.Pages.Add(page);
                this.context.SaveChanges();
                Visibility visibility = ((UIElement)this.list.Items[this.currentId]).Visibility;
                this.list.Items[this.currentId] = (object)this.GetListBoxItemWithCommands(page, 140, AnyConverter.IntToBool(page.IsTitle));
                ((UIElement)this.list.Items[this.currentId]).Visibility = visibility;
                CommonFunctions.ScrollTo(this.list, this.toOpenPage, this.CurrentId, this.CurrentId);
                if (title.Equals(page.Title) && isTitle == page.IsTitle && color.Equals(page.Color))
                    return;
                if (isTitle != page.IsTitle)
                {
                    this.UpdateTitlesOrder();
                    this.UpdateTitles();
                    switch (page.IsTitle)
                    {
                        case 0:
                            this.SetAllPages();
                            break;
                        case 1:
                            this.CommandOfTitlesItem(this.titlesOrder.IndexOf(page.PageOrder));
                            break;
                    }
                }
                else
                {
                    if (title.Equals(page.Title) && color.Equals(page.Color) || page.IsTitle != 1 || isTitle != page.IsTitle)
                        return;
                    int num3 = this.titlesOrder.IndexOf(page.PageOrder);
                    this.UpdateTitle(page, num3);
                    if (this.currentTitleId != num3)
                        return;
                    this.CommandOfTitlesItem(num3);
                }
            }
        }

        private void EditBookName(object sender, MouseButtonEventArgs e)
        {
            this.browser.Address = "null";
            BookNameEditor bookNameEditor = new BookNameEditor(this.path);
            bookNameEditor.Owner = (Window)this;
            bookNameEditor.ShowDialog();
            string[] strArray = this.path.Split('\\');
            this.NameBook.Text = strArray[strArray.Length - 1];
            this.unsaved = new DatabaseContext(this.path + "\\database\\UnsavedPagesDB.db");
            this.saved = new DatabaseContext(this.path + "\\database\\PagesDB.db");
            this.BookHasBeenChanged = this.BookHasBeenChanged;
            if (this.CurrentId < 0)
                return;
            this.ActionSelect();
        }

        private void AddFile(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            bool? nullable = openFileDialog.ShowDialog();
            bool flag = true;
            if (!(nullable.GetValueOrDefault() == flag & nullable.HasValue))
                return;
            string fileName = openFileDialog.FileName;
            File.Copy(fileName, System.IO.Path.Combine(this.path + "\\data", System.IO.Path.GetFileName(fileName)), true);
        }

        private void OnPreviewKeyUp(object obj, KeyEventArgs arg)
        {
            switch (this.hoveringListBox)
            {
                case -1:
                    if (arg.Key != Key.RightCtrl && arg.Key != Key.LeftCtrl)
                        break;
                    this.isControlKeyPressed = false;
                    break;
                case 0:
                    this.CopyPasteTitles(arg);
                    break;
                case 1:
                    this.CopyPasteList(arg);
                    break;
            }
        }

        private void CopyPasteTitles(KeyEventArgs arg)
        {
            try
            {
                if (this.titles.Items.Count == 1)
                    this.hoveringIdTitles = 1;
                if (this.hoveringIdTitles == -1)
                    return;
                int first = 0;
                int last = 0;
                if (this.titles.Items.Count != 1)
                {
                    last = this.list.Items.Count - 1;
                    if (this.hoveringIdTitles + 1 < this.titlesOrder.Count)
                        last = this.titlesOrder[this.hoveringIdTitles + 1] - 2;
                    first = this.titlesOrder[this.hoveringIdTitles] - 1;
                }
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    if (arg.Key.Equals((object)Key.C))
                    {
                        if (this.titles.Items.Count == 1)
                            return;
                        this.CopyTitles(first, last);
                    }
                    else if (arg.Key.Equals((object)Key.V))
                    {
                        if (this.copiedPages == null)
                            return;
                        int num1 = 1;
                        if (Keyboard.IsKeyDown(Key.LeftShift))
                            num1 = 0;
                        if (this.titles.Items.Count == 1)
                            num1 = 1;
                        int num2 = 0;
                        switch (num1)
                        {
                            case 0:
                                num2 = 0;
                                if (this.hoveringIdTitles > 1)
                                {
                                    num2 = this.titlesOrder[this.hoveringIdTitles] - 1;
                                    break;
                                }
                                break;
                            case 1:
                                num2 = this.list.Items.Count;
                                if (this.hoveringIdTitles + 1 < this.titlesOrder.Count)
                                {
                                    num2 = this.titlesOrder[this.hoveringIdTitles + 1] - 1;
                                    break;
                                }
                                break;
                        }
                        for (int index = 0; index < this.copiedPages.Count; ++index)
                        {
                            Pages page = (Pages)this.copiedPages[index].Clone();
                            page.PageOrder = num2 + index + 1;
                            this.list.Items.Insert(page.PageOrder - 1, (object)this.GetListBoxItemWithCommands(page, 140, AnyConverter.IntToBool(page.IsTitle)));
                            this.InsertPage(page);
                        }
                        if (this.titles.Items.Count == 1)
                            this.hoveringIdTitles = -1;
                        this.UpdateTitles();
                        this.CommandOfTitlesItem(this.titlesOrder.IndexOf(num2 + 1));
                    }
                    else
                    {
                        if (!arg.Key.Equals((object)Key.X) || this.titles.Items.Count == 1)
                            return;
                        this.CopyTitles(first, last);
                        this.DeleteTitles(first, last);
                    }
                }
                else
                {
                    if (!arg.Key.Equals((object)Key.Delete) || this.titles.Items.Count == 1)
                        return;
                    this.DeleteTitles(first, last);
                }
            }
            catch (Exception ex)
            {
                int num = (int)System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void CopyTitles(int first, int last)
        {
            this.copiedPages = new List<Pages>();
            for (int index = first; index <= last; ++index)
                this.copiedPages.Add(this.context.GetPage(index + 1));
        }

        private void DeleteTitles(int first, int last)
        {
            for (int removeIndex = last; removeIndex > first; --removeIndex)
            {
                this.DeletePage(removeIndex + 1);
                this.list.Items.RemoveAt(removeIndex);
            }
            this.CurrentId = first;
            this.ActionDelete();
        }

        private void CopyPasteList(KeyEventArgs arg)
        {
            try
            {
                if (this.CanPasteToEmptyTitle())
                    this.hoveringIdList = this.titlesOrder[this.currentTitleId] - 1;
                if (this.list.Items.Count == 0)
                    this.hoveringIdList = 0;
                if (this.hoveringIdList == -1)
                    return;
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    Key key = arg.Key;
                    if (key.Equals((object)Key.C))
                    {
                        if (this.list.Items.Count == 0 || this.CanPasteToEmptyTitle())
                            return;
                        this.copiedPage = this.context.GetPage(this.hoveringIdList + 1);
                    }
                    else
                    {
                        key = arg.Key;
                        if (key.Equals((object)Key.V))
                        {
                            if (this.copiedPage == null)
                                return;
                            int num = 1;
                            if (Keyboard.IsKeyDown(Key.LeftShift) && !this.CanPasteToEmptyTitle() || this.list.Items.Count == 0)
                                num = 0;
                            Pages page = (Pages)this.copiedPage.Clone();
                            this.CurrentId = this.hoveringIdList + num;
                            if (this.list.Items.Count == 0 || this.CanPasteToEmptyTitle())
                                this.hoveringIdList = -1;
                            page.PageOrder = this.CurrentId + 1;
                            this.list.Items.Insert(this.CurrentId, (object)this.GetListBoxItemWithCommands(page, 140, AnyConverter.IntToBool(page.IsTitle)));
                            this.InsertPage(page);
                            if (page.IsTitle == 1)
                            {
                                this.UpdateTitles();
                                this.CommandOfTitlesItem(this.titlesOrder.IndexOf(page.PageOrder));
                            }
                            else
                                this.ActionSelect();
                        }
                        else
                        {
                            key = arg.Key;
                            if (!key.Equals((object)Key.X) || this.list.Items.Count == 0 || this.CanPasteToEmptyTitle())
                                return;
                            this.copiedPage = this.context.GetPage(this.hoveringIdList + 1);
                            this.CurrentId = this.hoveringIdList;
                            this.ActionDelete();
                        }
                    }
                }
                else
                {
                    if (!arg.Key.Equals((object)Key.Delete) || this.list.Items.Count == 0 || this.CanPasteToEmptyTitle())
                        return;
                    this.CurrentId = this.hoveringIdList;
                    this.ActionDelete();
                }
            }
            catch (Exception ex)
            {
                int num = (int)System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private bool CanPasteToEmptyTitle()
        {
            return this.currentTitleId != 0 && this.WayToNextTitle(this.titlesOrder[this.currentTitleId]) == 0;
        }

        private void Search(object sender, KeyEventArgs e)
        {
            CommonFunctions.Search(this.CurrentId, this.currentTitleId, this.searchBox.Text, ref this.searchedPageId, ref this.searchingName, ref this.titlesOrder, this.list, this.toOpenPage);
        }

        private void SaveFile(object sender, MouseButtonEventArgs e)
        {
            if (!this.BookHasBeenChanged)
                return;
            this.Save();
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
                if (System.Windows.MessageBox.Show(ex.Message, "Непредвиденная ошибка", MessageBoxButton.OK, MessageBoxImage.Hand) == MessageBoxResult.None)
                    ;
            }
        }

        private void AddListBoxItemInList(Pages page)
        {
            if (AnyConverter.IntToBool(page.IsTitle))
            {
                this.titlesOrder.Add(page.PageOrder);
                this.AddTitlesItemWithCommand((Pages)page.Clone(), this.titlesOrder.Count - 1);
            }
            this.list.Items.Add((object)this.GetListBoxItemWithCommands(page, 140, AnyConverter.IntToBool(page.IsTitle)));
        }

        private void AddTitlesItemWithCommand(Pages page, int ord)
        {
            this.titles.Items.Add((object)this.GetTitlesItem(page, ord));
        }

        private ListBoxItem GetTitlesItem(Pages page, int ord)
        {
            ListBoxItem listBoxItem = this.GetListBoxItem(page, 140, false);
            listBoxItem.MouseLeftButtonUp += (MouseButtonEventHandler)((sender, e) => this.CommandOfTitlesItem(ord));
            listBoxItem.MouseEnter += (MouseEventHandler)((sender, e) => this.hoveringIdTitles = this.titles.Items.IndexOf(sender));
            listBoxItem.MouseLeave += (MouseEventHandler)((sender, e) => this.hoveringIdTitles = -1);
            return listBoxItem;
        }

        private void CommandOfTitlesItem(int ord)
        {
            int nextTitle = this.WayToNextTitle(this.titlesOrder[ord]);
            CommonFunctions.ScrollTo(this.titles, this.toOpenPage, ord, this.CurrentId, true);
            CommonFunctions.ShowPages(this.titlesOrder[ord], nextTitle, this.list);
            this.currentTitleId = ord;
            this.CurrentId = this.titlesOrder[ord] - 1;
            this.ActionSelect();
        }

        private void AddAllPages()
        {
            if (this.titlesOrder.Count != 0)
                return;
            ListBoxItem listBoxItem = this.GetListBoxItem("Все страницы", 140);
            listBoxItem.MouseLeftButtonUp += (MouseButtonEventHandler)((sender, e) => this.SetAllPages());
            this.titlesOrder.Add(0);
            this.titles.Items.Add((object)listBoxItem);
        }

        private void SetAllPages()
        {
            CommonFunctions.ScrollTo(this.titles, this.toOpenPage, 0, this.CurrentId, true);
            CommonFunctions.ShowPages(0, this.list.Items.Count, this.list);
            this.currentTitleId = 0;
        }

        private void UpdateTitlesOrder()
        {
            this.titlesOrder.Clear();
            this.titlesOrder.Add(0);
            this.titlesOrder.AddRange((IEnumerable<int>)this.context.Pages.OrderBy<Pages, int>((Expression<Func<Pages, int>>)(x => x.PageOrder)).Where<Pages>((Expression<Func<Pages, bool>>)(x => x.IsTitle == 1)).Select<Pages, int>((Expression<Func<Pages, int>>)(x => x.PageOrder)).ToList<int>());
        }

        private void UpdateTitles()
        {
            for (int removeIndex = this.titles.Items.Count - 1; removeIndex > 0; --removeIndex)
                this.titles.Items.RemoveAt(removeIndex);
            for (int index = 1; index < this.titlesOrder.Count; ++index)
                this.AddTitlesItemWithCommand(this.context.GetPage(this.titlesOrder[index]), index);
        }

        private void UpdateTitle(Pages page, int idTitle)
        {
            this.titles.Items[idTitle] = (object)this.GetTitlesItem(page, idTitle);
        }

        private ListBoxItem GetListBoxItemWithCommands(Pages page, int buttonWidth, bool isTitle)
        {
            ListBoxItem listBoxItem = this.GetListBoxItem(page, buttonWidth, isTitle);
            listBoxItem.MouseLeftButtonUp += (MouseButtonEventHandler)((sender, e) => this.Action((ListBoxItem)sender));
            listBoxItem.MouseEnter += (MouseEventHandler)((sender, e) => this.hoveringIdList = this.list.Items.IndexOf(sender));
            listBoxItem.MouseLeave += (MouseEventHandler)((sender, e) => this.hoveringIdList = -1);
            return listBoxItem;
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

        private void PerformInitialSteps(string path, bool bookHasBeenChanged)
        {
            try
            {
                this.Closing += new CancelEventHandler(this.BookEditor_Closing);
                this.path = path;
                this.unsaved = new DatabaseContext(path + "\\database\\UnsavedPagesDB.db");
                this.saved = new DatabaseContext(path + "\\database\\PagesDB.db");
                this.BookHasBeenChanged = bookHasBeenChanged;
                this.browser.DownloadHandler = (IDownloadHandler)new Heap.Behaviors.DownloadHandler();
                this.browser.LifeSpanHandler = (ILifeSpanHandler)new CustomLifeSpanHandler();
                this.browser.MenuHandler = (IContextMenuHandler)new CustomMenuHandler();
                string[] strArray = path.Split('\\');
                this.NameBook.Text = strArray[strArray.Length - 1];
                this.titlesOrder = new List<int>();
                this.titles.MouseEnter += (MouseEventHandler)((sender, e) => this.hoveringListBox = 0);
                this.titles.MouseLeave += (MouseEventHandler)((sender, e) => this.hoveringListBox = -1);
                this.list.MouseEnter += (MouseEventHandler)((sender, e) => this.hoveringListBox = 1);
                this.list.MouseLeave += (MouseEventHandler)((sender, e) => this.hoveringListBox = -1);
            }
            catch (Exception ex)
            {
                int num = (int)System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void OnColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            if (this.CurrentId == -1)
                return;
            this.color.Text = this.colorPicker.SelectedColor.ToString();
        }

        private bool IsNotDatabase(string brsAdrStr, string p)
        {
            return p.Length == brsAdrStr.Length || brsAdrStr[p.Length] == '/';
        }

        private bool BookHasBeenChanged
        {
            get => this.bookHasBeenChanged;
            set
            {
                try
                {
                    if (this.context != null)
                        this.context.SaveChanges();
                    if (value)
                    {
                        this.saveImageButton.Source = (ImageSource)new BitmapImage(new Uri("/ElectronicBooks;component/Resources/Images/saveicon.png", UriKind.Relative));
                        this.context = this.unsaved;
                        if (this.context.GetPagesCount() == 0)
                        {
                            if (!this.bookHasBeenChanged)
                            {
                                this.unsaved.Pages.AddRange((IEnumerable<Pages>)this.saved.Pages);
                                this.unsaved.SaveChanges();
                                this.generatedId = this.context.GetPagesCount();
                            }
                        }
                    }
                    else
                    {
                        this.saveImageButton.Source = (ImageSource)new BitmapImage(new Uri("/ElectronicBooks;component/Resources/Images/inactivesaveicon.png", UriKind.Relative));
                        this.context = this.saved;
                    }
                }
                catch (Exception ex)
                {
                    int num = (int)System.Windows.MessageBox.Show(ex.Message);
                }
                this.bookHasBeenChanged = value;
            }
        }

        private int GetGeneratedId()
        {
            ++this.generatedId;
            return this.generatedId;
        }

        private int CurrentId
        {
            get => this.currentId;
            set
            {
                if (this.list.Items.Count == 0 && value < 0)
                {
                    this.title.Text = "null";
                    this.link.Text = "null";
                    this.browser.Address = this.link.Text;
                    this.colorPicker.SelectedColor = new System.Windows.Media.Color?();
                    this.color.Text = "null";
                    this.isTitle.IsChecked = new bool?(false);
                    this.CurrentTypeLink = 2;
                    this.currentId = -1;
                    this.editorPage.Visibility = Visibility.Collapsed;
                    this.toOpenPage.Background = (Brush)AnyConverter.GetBrush("#FFC6C6C6");
                    this.toOpenPage.Foreground = (Brush)AnyConverter.GetBrush("#7E7E7E");
                }
                else if (value < 0)
                {
                    int num = 0;
                    if (this.currentId > this.context.GetPagesCount() - 1 && this.currentTitleId == 0 || (this.currentTitleId + 1 < this.titlesOrder.Count && this.currentId == this.titlesOrder[this.currentTitleId + 1] - 1 || this.currentId == this.list.Items.Count) && this.currentTitleId != 0)
                        num = -1;
                    this.currentId += num;
                    this.ActionSelect();
                }
                else
                {
                    if (this.editorPage.Visibility == Visibility.Collapsed)
                        this.editorPage.Visibility = Visibility.Visible;
                    this.currentId = value;
                }
            }
        }

        private int CurrentTypeLink
        {
            get => this.currentTypeLink;
            set
            {
                this.currentTypeLink = value;
                this.typeLink.SelectedIndex = this.currentTypeLink;
            }
        }

        private void OnClickAddElement(object sender, MouseButtonEventArgs e)
        {
            this.elementsList.SelectedIndex = -1;
            this.ActionAdd(this.elementsList.Items.IndexOf(sender));
        }

        private int WayToNextTitle(int curentOrder)
        {
            return this.context.GetNextPagesTitleOrder(curentOrder) - curentOrder - 1;
        }

        private void ToOpenedPage(object sender, MouseButtonEventArgs e)
        {
            if (this.CurrentId < 0)
                return;
            CommonFunctions.ScrollTo(this.list, this.toOpenPage, this.CurrentId, this.CurrentId);
            CommonFunctions.ScrollTo(this.titles, this.toOpenPage, this.currentTitleId, this.CurrentId, true);
        }

        private void DeletePageInList(object sender, MouseButtonEventArgs e)
        {
            if (this.CurrentId < 0)
                return;
            this.ActionDelete();
        }

        private void ClearUnusedFiles(object sender, MouseButtonEventArgs e)
        {
            if (this.BookHasBeenChanged)
            {
                switch (System.Windows.MessageBox.Show("Чтобы очистить неиспользуемые файлы, нужно сначала сохранить книгу. Желаете ее сохранить?", "Сохраните книгу", MessageBoxButton.YesNo, MessageBoxImage.Asterisk))
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
            new ClearUnusedDocuments(this.path).ShowDialog();
        }

        private void PasswordMenu(object sender, MouseButtonEventArgs e)
        {
            if (this.BookHasBeenChanged)
            {
                switch (System.Windows.MessageBox.Show("Чтобы изменить пароль, нужно сначала сохранить книгу. Желаете ее сохранить?", "Сохраните книгу", MessageBoxButton.YesNo, MessageBoxImage.Asterisk))
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
            new PasswordWindow(this.path).ShowDialog();
        }

        private void OnInfo(object sender, MouseButtonEventArgs e)
        {
            try
            {
                new BookViewer(AppSettings.AppDirectoryPath() + "\\Руководство пользователя", isInfoAboutProgram: true).ShowDialog();
            }
            catch (Exception ex)
            {
                int num = (int)System.Windows.MessageBox.Show(ex.Message);
            }
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

        private void onEditDescription(object sender, MouseButtonEventArgs e)
        {
            if (this.BookHasBeenChanged)
            {
                switch (System.Windows.MessageBox.Show("Чтобы открыть редактор описания книги, нужно сначала сохранить книгу. Желаете ее сохранить?", "Сохраните книгу", MessageBoxButton.YesNo, MessageBoxImage.Asterisk))
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
            EditBookDescription editBookDescription = new EditBookDescription(this.saved, this.NameBook.Text);
            editBookDescription.Owner = (Window)this;
            editBookDescription.ShowDialog();
        }

    }
}
